using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // Set default global options
        EnablePlugin = true;
        ScanThreads = 2;
        LogLevel = LogLevel.Info;
        AuditRetentionDays = 30;
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
    public int ScanThreads { get; set; }

    /// <summary>
    /// Gets or sets the logging verbosity level.
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// Gets or sets the number of days to retain deletion audit logs.
    /// </summary>
    public int AuditRetentionDays { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to preview deletions without executing.
    /// </summary>
    public bool DryRunMode { get; set; }

    /// <summary>
    /// Gets or sets the per-library preferences dictionary.
    /// </summary>
    public IDictionary<string, LibraryPreferences> LibraryPreferences { get; set; }
}
