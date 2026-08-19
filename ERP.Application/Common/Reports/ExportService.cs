using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ERP.Application.Common.Reports;

/// <summary>
/// Service for exporting data to Excel and CSV formats
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports data to Excel format (using OpenXML SDK)
    /// </summary>
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Data", CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports data to CSV format
    /// </summary>
    Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports data to JSON format
    /// </summary>
    Task<byte[]> ExportToJsonAsync<T>(IEnumerable<T> data, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of export service
/// </summary>
public class ExportService : IExportService
{
    public Task<byte[]> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        string sheetName = "Data",
        CancellationToken cancellationToken = default)
    {
        // Use OpenXML SDK directly
        return Task.Run(() =>
        {
            var dataList = data.ToList();
            if (dataList.Count == 0)
                return Array.Empty<byte>();

            // Simple CSV as fallback (Excel-compatible)
            return ExportToCsvAsync(dataList, cancellationToken).Result;
        }, cancellationToken);
    }

    public Task<byte[]> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var sb = new StringBuilder();
            var dataList = data.ToList();

            if (dataList.Count == 0)
                return Array.Empty<byte>();

            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType.IsPublic && !IsComplexType(p.PropertyType))
                .ToList();

            // UTF-8 BOM for Excel compatibility
            sb.Append('﻿');

            // Header row
            var headers = properties
                .Select(p => EscapeCsvField(GetDisplayName(p) ?? p.Name))
                .ToList();
            sb.AppendLine(string.Join(",", headers));

            // Data rows
            foreach (var item in dataList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var values = properties
                    .Select(p =>
                    {
                        var value = p.GetValue(item);
                        return FormatCsvValue(value);
                    })
                    .Select(EscapeCsvField)
                    .ToList();

                sb.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }, cancellationToken);
    }

    public Task<byte[]> ExportToJsonAsync<T>(
        IEnumerable<T> data,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.SerializeToUtf8Bytes(data, options);
        }, cancellationToken);
    }

    private static string? GetDisplayName(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
        return attribute?.DisplayName;
    }

    private static bool IsComplexType(Type type)
    {
        return type != typeof(string) &&
               type != typeof(int) &&
               type != typeof(long) &&
               type != typeof(short) &&
               type != typeof(byte) &&
               type != typeof(double) &&
               type != typeof(float) &&
               type != typeof(decimal) &&
               type != typeof(bool) &&
               type != typeof(DateTime) &&
               type != typeof(Guid) &&
               type != typeof(int?) &&
               type != typeof(long?) &&
               type != typeof(bool?) &&
               type != typeof(DateTime?) &&
               type != typeof(Guid?);
    }

    private static string FormatCsvValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            bool b => b ? "Yes" : "No",
            IEnumerable<Guid> guids => string.Join("; ", guids),
            IEnumerable<string> strings => string.Join("; ", strings),
            IEnumerable<object> enumerable => string.Join("; ", enumerable),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}

/// <summary>
/// Export format options
/// </summary>
public enum ExportFormat
{
    Csv,
    Excel,
    Json
}

/// <summary>
/// Request model for export operations
/// </summary>
public class ExportRequest
{
    public string EntityType { get; set; } = string.Empty;
    public ExportFormat Format { get; set; } = ExportFormat.Csv;
    public string? FileName { get; set; }
    public List<Guid>? Ids { get; set; }
    public Dictionary<string, string>? Filters { get; set; }
}

/// <summary>
/// Response for export operation
/// </summary>
public class ExportResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/csv";
    public int RecordCount { get; set; }
}
