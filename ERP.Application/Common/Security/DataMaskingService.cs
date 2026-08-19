using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERP.Application.Common.Security;

/// <summary>
/// Service for masking sensitive data in exports and logs
/// </summary>
public interface IDataMaskingService
{
    /// <summary>
    /// Masks sensitive fields in an object
    /// </summary>
    T MaskSensitiveData<T>(T data, DataMaskingOptions? options = null);

    /// <summary>
    /// Masks a string value based on masking type
    /// </summary>
    string MaskValue(string value, MaskingType type = MaskingType.Partial);

    /// <summary>
    /// Masks PII data in a dictionary (e.g., for exports)
    /// </summary>
    Dictionary<string, object?> MaskDictionary(Dictionary<string, object?> data, DataMaskingOptions? options = null);
}

/// <summary>
/// Data masking options
/// </summary>
public class DataMaskingOptions
{
    /// <summary>
    /// Default masking type for fields
    /// </summary>
    public MaskingType DefaultMaskingType { get; set; } = MaskingType.Partial;

    /// <summary>
    /// Fields to mask completely (no reveal)
    /// </summary>
    public HashSet<string> FullMaskFields { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwordhash", "secret", "apikey", "token", "privatekey"
    };

    /// <summary>
    /// Fields to partially mask (show first/last chars)
    /// </summary>
    public HashSet<string> PartialMaskFields { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "email", "phone", "address", "nik", "ktp", "passport"
    };

    /// <summary>
    /// Fields to mask completely for financial data
    /// </summary>
    public HashSet<string> FinancialMaskFields { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "bankaccount", "creditcard", "cardnumber", "cvv"
    };
}

/// <summary>
/// Masking type enumeration
/// </summary>
public enum MaskingType
{
    /// <summary>Show first and last characters</summary>
    Partial,

    /// <summary>Show only last N characters</summary>
    LastOnly,

    /// <summary>Show only first N characters</summary>
    FirstOnly,

    /// <summary>Replace with asterisks</summary>
    Full,

    /// <summary>Replace with random characters</summary>
    Random
}

/// <summary>
/// Attribute to mark a property for masking
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MaskDataAttribute : Attribute
{
    public MaskingType Type { get; }
    public int RevealChars { get; }

    public MaskDataAttribute(MaskingType type = MaskingType.Partial, int revealChars = 4)
    {
        Type = type;
        RevealChars = revealChars;
    }
}

/// <summary>
/// Default implementation of data masking service
/// </summary>
public class DataMaskingService : IDataMaskingService
{
    private readonly DataMaskingOptions _options;

    public DataMaskingService(DataMaskingOptions? options = null)
    {
        _options = options ?? new DataMaskingOptions();
    }

    public T MaskSensitiveData<T>(T data, DataMaskingOptions? options = null)
    {
        var opts = options ?? _options;

        if (data == null)
            return default!;

        var type = typeof(T);

        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            return data;

        // Handle anonymous types and dictionaries
        if (data is Dictionary<string, object?> dict)
            return (T)(object)MaskDictionary(dict, opts)!;

        // Handle collections
        if (data is IEnumerable<object> collection)
        {
            var maskedList = new List<object?>();
            foreach (var item in collection)
            {
                if (item is Dictionary<string, object?> itemDict)
                    maskedList.Add(MaskDictionary(itemDict, opts));
                else
                    maskedList.Add(MaskObject(item, opts));
            }
            return (T)(object)maskedList;
        }

        // For other objects, return as dictionary
        var result = MaskObject(data, opts);
        if (result is Dictionary<string, object?> dictResult)
        {
            return (T)(object)dictResult;
        }

        return data;
    }

    public string MaskValue(string value, MaskingType type = MaskingType.Partial)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return type switch
        {
            MaskingType.Full => new string('*', value.Length),
            MaskingType.Partial => MaskPartial(value, 3),
            MaskingType.LastOnly => MaskLastOnly(value, 4),
            MaskingType.FirstOnly => MaskFirstOnly(value, 4),
            MaskingType.Random => MaskRandom(value),
            _ => value
        };
    }

    public Dictionary<string, object?> MaskDictionary(Dictionary<string, object?> data, DataMaskingOptions? options = null)
    {
        var opts = options ?? _options;
        var result = new Dictionary<string, object?>();

        foreach (var kvp in data)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            if (value == null)
            {
                result[key] = null;
                continue;
            }

            if (ShouldMaskField(key, opts))
            {
                if (opts.FullMaskFields.Contains(key))
                {
                    result[key] = "***MASKED***";
                }
                else if (opts.PartialMaskFields.Contains(key) || opts.FinancialMaskFields.Contains(key))
                {
                    result[key] = MaskValue(value.ToString() ?? string.Empty, MaskingType.Partial);
                }
                else
                {
                    result[key] = MaskValue(value.ToString() ?? string.Empty, opts.DefaultMaskingType);
                }
            }
            else
            {
                result[key] = value;
            }
        }

        return result;
    }

    private object? MaskObject(object obj, DataMaskingOptions options)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            if (dict == null)
                return obj;

            var result = new Dictionary<string, object?>();
            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                if (ShouldMaskField(key, options))
                {
                    var stringValue = value.ValueKind == JsonValueKind.String
                        ? value.GetString()
                        : value.GetRawText();

                    if (stringValue == null)
                    {
                        result[key] = null;
                        continue;
                    }

                    if (options.FullMaskFields.Contains(key))
                    {
                        result[key] = "***MASKED***";
                    }
                    else if (options.PartialMaskFields.Contains(key) || options.FinancialMaskFields.Contains(key))
                    {
                        result[key] = MaskValue(stringValue, MaskingType.Partial);
                    }
                    else
                    {
                        result[key] = MaskValue(stringValue, options.DefaultMaskingType);
                    }
                }
                else
                {
                    result[key] = value.ValueKind == JsonValueKind.Null ? null : value.GetRawText();
                }
            }

            return result;
        }
        catch
        {
            return obj;
        }
    }

    private bool ShouldMaskField(string fieldName, DataMaskingOptions options)
    {
        return options.FullMaskFields.Contains(fieldName) ||
               options.PartialMaskFields.Contains(fieldName) ||
               options.FinancialMaskFields.Contains(fieldName);
    }

    private static string MaskPartial(string value, int revealChars)
    {
        if (value.Length <= revealChars * 2)
            return new string('*', value.Length);

        var first = value[..revealChars];
        var last = value[^revealChars..];
        var middle = new string('*', Math.Max(4, value.Length - revealChars * 2));

        return $"{first}{middle}{last}";
    }

    private static string MaskLastOnly(string value, int revealChars)
    {
        if (value.Length <= revealChars)
            return new string('*', value.Length);

        return new string('*', value.Length - revealChars) + value[^revealChars..];
    }

    private static string MaskFirstOnly(string value, int revealChars)
    {
        if (value.Length <= revealChars)
            return new string('*', value.Length);

        return value[..revealChars] + new string('*', value.Length - revealChars);
    }

    private static string MaskRandom(string value)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, value.Length)
            .Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}

/// <summary>
/// Extension methods for data masking
/// </summary>
public static class DataMaskingExtensions
{
    /// <summary>
    /// Masks PII fields in an object for GDPR compliance
    /// </summary>
    public static T MaskForGdpr<T>(this T data)
    {
        var service = new DataMaskingService(new DataMaskingOptions
        {
            FullMaskFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "password", "passwordhash", "secret", "token", "privatekey", "creditcard", "cvv"
            },
            PartialMaskFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "email", "phone", "address", "nik", "ktp", "passport", "bankaccount"
            }
        });

        return service.MaskSensitiveData(data);
    }

    /// <summary>
    /// Masks PII fields in a dictionary for GDPR compliance
    /// </summary>
    public static Dictionary<string, object?> MaskForGdpr(this Dictionary<string, object?> data)
    {
        var service = new DataMaskingService();
        return service.MaskDictionary(data);
    }
}
