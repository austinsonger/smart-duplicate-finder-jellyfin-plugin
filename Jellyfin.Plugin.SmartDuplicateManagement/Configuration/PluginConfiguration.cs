using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    private int _scanThreads;
    private int _auditRetentionDays;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // Set default global options
        EnablePlugin = true;
        _scanThreads = 2;
        LogLevel = LogLevel.Info;
        _auditRetentionDays = 30;
        DryRunMode = false;
        LibraryPreferences = new Dictionary<string, LibraryPreferences>();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled.
    /// </summary>
    public bool EnablePlugin { get; set; }

    /// <summary>
    /// Gets or sets the number of parallel threads for duplicate detection.
    /// </summary>
    [Range(1, 8)]
    public int ScanThreads
    {
        get => _scanThreads;
        set => _scanThreads = value < 1 ? 1 : (value > 8 ? 8 : value);
    }

    /// <summary>
    /// Gets or sets the logging verbosity level.
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// Gets or sets the number of days to retain deletion audit logs.
    /// </summary>
    [Range(1, 365)]
    public int AuditRetentionDays
    {
        get => _auditRetentionDays;
        set => _auditRetentionDays = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to preview deletions without executing.
    /// </summary>
    public bool DryRunMode { get; set; }

    /// <summary>
    /// Gets the per-library preferences dictionary.
    /// </summary>
    public IDictionary<string, LibraryPreferences> LibraryPreferences { get; }
}
