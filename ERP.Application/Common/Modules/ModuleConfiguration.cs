using System.Text.Json;

namespace ERP.Application.Common.Modules;

/// <summary>
/// Module configuration loaded from manifest and module configs.
/// </summary>
public class ModuleConfiguration
{
    public string Module { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Tier { get; set; } = string.Empty;
    public Dictionary<string, FeatureConfig> Features { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();
}

/// <summary>
/// Individual feature configuration.
/// </summary>
public class FeatureConfig
{
    public bool Enabled { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Central module manifest.
/// </summary>
public class ModuleManifest
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, ModuleManifestEntry> Modules { get; set; } = new();
    public Dictionary<string, TierInfo> Tiers { get; set; } = new();
    public LicenseConfig License { get; set; } = new();
}

/// <summary>
/// Module entry in manifest.
/// </summary>
public class ModuleManifestEntry
{
    public bool Enabled { get; set; }
    public string Tier { get; set; } = string.Empty;
    public int TierOrder { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
}

/// <summary>
/// Tier information.
/// </summary>
public class TierInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Modules { get; set; } = new();
}

/// <summary>
/// License configuration.
/// </summary>
public class LicenseConfig
{
    public string DefaultTier { get; set; } = "starter";
    public int TrialDays { get; set; } = 14;
    public int WarningDays { get; set; } = 7;
}

/// <summary>
/// Module configuration loader service.
/// </summary>
public static class ModuleConfigurationLoader
{
    private static ModuleManifest? _manifest;
    private static readonly Dictionary<string, ModuleConfiguration> _moduleConfigs = new();
    private static readonly object _lock = new();

    /// <summary>
    /// Load module manifest from JSON file.
    /// </summary>
    public static ModuleManifest LoadManifest(string path = "modules/module-manifest.json")
    {
        if (_manifest != null) return _manifest;

        var fullPath = Path.Combine(AppContext.BaseDirectory, path);
        if (!File.Exists(fullPath))
        {
            // Try relative to current directory
            fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
        }

        if (File.Exists(fullPath))
        {
            var json = File.ReadAllText(fullPath);
            _manifest = JsonSerializer.Deserialize<ModuleManifest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return _manifest ?? new ModuleManifest();
    }

    /// <summary>
    /// Load module configuration for a specific module.
    /// </summary>
    public static ModuleConfiguration LoadModuleConfig(string moduleName, string basePath = "modules")
    {
        var cacheKey = moduleName.ToLower();
        if (_moduleConfigs.TryGetValue(cacheKey, out var cached))
            return cached;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), basePath, moduleName, "module.config.json");
        if (!File.Exists(fullPath))
        {
            return new ModuleConfiguration { Module = moduleName, Enabled = false };
        }

        var json = File.ReadAllText(fullPath);
        var config = JsonSerializer.Deserialize<ModuleConfiguration>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config != null)
        {
            lock (_lock)
            {
                _moduleConfigs[cacheKey] = config;
            }
        }

        return config ?? new ModuleConfiguration { Module = moduleName, Enabled = false };
    }

    /// <summary>
    /// Get all available modules with their status.
    /// </summary>
    public static List<ModuleConfiguration> GetAllModules()
    {
        var manifest = LoadManifest();
        var result = new List<ModuleConfiguration>();

        foreach (var (name, entry) in manifest.Modules)
        {
            var config = LoadModuleConfig(name);
            config.Enabled = entry.Enabled && config.Enabled;
            result.Add(config);
        }

        return result;
    }

    /// <summary>
    /// Check if a module is enabled.
    /// </summary>
    public static bool IsModuleEnabled(string moduleCode)
    {
        var manifest = LoadManifest();
        var entry = manifest.Modules.Values.FirstOrDefault(m =>
            m.Path.Contains(moduleCode, StringComparison.OrdinalIgnoreCase));

        if (entry == null) return false;

        var config = LoadModuleConfig(entry.Path.Split('/').Last());
        return entry.Enabled && config.Enabled;
    }

    /// <summary>
    /// Check if a feature is enabled within a module.
    /// </summary>
    public static bool IsFeatureEnabled(string moduleCode, string featureName)
    {
        var config = LoadModuleConfig(moduleCode);
        if (!config.Enabled) return false;

        if (config.Features.TryGetValue(featureName, out var feature))
            return feature.Enabled;

        return true; // Feature enabled by default if not specified
    }
}
