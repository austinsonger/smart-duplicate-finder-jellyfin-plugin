using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.SmartDuplicateManagement.Services;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Tasks;

/// <summary>
/// Scheduled task for scanning libraries for duplicates.
/// </summary>
public class DuplicateScanTask : IScheduledTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<DuplicateScanTask> _logger;
    private readonly DuplicateDetectionEngine _detectionEngine;
    private readonly QualityAnalyzer _qualityAnalyzer;
    private readonly MetadataMerger _metadataMerger;
    private readonly DataPersistenceService _persistenceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateScanTask"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="detectionEngine">The detection engine.</param>
    /// <param name="qualityAnalyzer">The quality analyzer.</param>
    /// <param name="metadataMerger">The metadata merger.</param>
    /// <param name="persistenceService">The persistence service.</param>
    public DuplicateScanTask(
        ILibraryManager libraryManager,
        ILogger<DuplicateScanTask> logger,
        DuplicateDetectionEngine detectionEngine,
        QualityAnalyzer qualityAnalyzer,
        MetadataMerger metadataMerger,
        DataPersistenceService persistenceService)
    {
        _libraryManager = libraryManager;
        _logger = logger;
        _detectionEngine = detectionEngine;
        _qualityAnalyzer = qualityAnalyzer;
        _metadataMerger = metadataMerger;
        _persistenceService = persistenceService;
    }

    /// <inheritdoc />
    public string Name => "Scan for Duplicate Media";

    /// <inheritdoc />
    public string Key => "SmartDuplicateScan";

    /// <inheritdoc />
    public string Description => "Scans libraries for duplicate media files and analyzes quality";

    /// <inheritdoc />
    public string Category => "Smart Duplicate Management";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting duplicate scan task");

        using var scanLock = _persistenceService.AcquireScanLock();
        if (scanLock == null)
        {
            _logger.LogWarning("Another scan is already in progress. Skipping scheduled task.");
            return;
        }

        try
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null || !config.EnablePlugin)
            {
                _logger.LogInformation("Plugin is disabled, skipping scan");
                return;
            }

            // Get all libraries
            var libraries = _libraryManager.GetVirtualFolders();
            var libraryCount = libraries.Count;

            if (libraryCount == 0)
            {
                _logger.LogInformation("No libraries found");
                return;
            }

            for (int i = 0; i < libraryCount; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var library = libraries[i];
                _logger.LogInformation("Scanning library: {Name}", library.Name);

                if (!Guid.TryParse(library.ItemId, out var libraryId))
                {
                    _logger.LogError("Invalid library item ID format: {ItemId} for library {Name}. Skipping.", library.ItemId, library.Name);
                    continue;
                }

                // Check if library has preferences configured
                if (!config.LibraryPreferences.TryGetValue(library.ItemId, out var preferences))
                {
                    // Use default preferences
                    preferences = new Configuration.LibraryPreferences
                    {
                        LibraryId = library.ItemId
                    };
                }

                // Detect duplicates
                var duplicates = await _detectionEngine.ScanLibraryAsync(
                    libraryId,
                    preferences,
                    cancellationToken).ConfigureAwait(false);

                // Analyze quality and merge metadata for each group
                foreach (var group in duplicates)
                {
                    _qualityAnalyzer.AnalyzeQuality(group, preferences);
                    _metadataMerger.MergeMetadata(group);
                }

                // Save results
                await _persistenceService.SaveDuplicateGroupsAsync(
                    libraryId,
                    duplicates).ConfigureAwait(false);

                // Update progress
                progress.Report(((double)(i + 1) / libraryCount) * 100);
            }

            _logger.LogInformation("Duplicate scan task completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing duplicate scan task");
            throw;
        }
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // No automatic triggers - manual execution only
        return Enumerable.Empty<TaskTriggerInfo>();
    }
}
