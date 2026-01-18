using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.SmartDuplicateManagement.Configuration;
using Jellyfin.Plugin.SmartDuplicateManagement.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Services;

/// <summary>
/// Service for detecting duplicate media items across libraries.
/// </summary>
public class DuplicateDetectionEngine
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<DuplicateDetectionEngine> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateDetectionEngine"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    public DuplicateDetectionEngine(
        ILibraryManager libraryManager,
        ILogger<DuplicateDetectionEngine> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    /// <summary>
    /// Scans a library for duplicate media items.
    /// </summary>
    /// <param name="libraryId">The library identifier.</param>
    /// <param name="preferences">The library preferences.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of detected duplicate groups.</returns>
    public async Task<List<DuplicateGroup>> ScanLibraryAsync(
        Guid libraryId,
        LibraryPreferences preferences,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting duplicate scan for library {LibraryId}", libraryId);

        var duplicateGroups = new List<DuplicateGroup>();

        try
        {
            // Get all items from the library
            var items = GetLibraryItems(libraryId);
            _logger.LogInformation("Found {Count} items in library", items.Count);

            // Stage 1: Initial Grouping by normalized title
            var titleGroups = GroupByNormalizedTitle(items);
            _logger.LogInformation("Grouped into {Count} potential duplicate sets", titleGroups.Count);

            // Stage 2: Similarity Scoring and Duplicate Detection
            foreach (var group in titleGroups)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (group.Value.Count < 2)
                {
                    continue;
                }

                var duplicates = DetectDuplicatesInGroup(group.Value, preferences);
                if (duplicates != null && duplicates.Versions.Count >= 2)
                {
                    duplicates.LibraryId = libraryId;
                    duplicateGroups.Add(duplicates);
                }
            }

            _logger.LogInformation("Detected {Count} duplicate groups", duplicateGroups.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning library {LibraryId}", libraryId);
        }

        return duplicateGroups;
    }

    /// <summary>
    /// Gets all movies and TV shows from a library.
    /// </summary>
    private List<BaseItem> GetLibraryItems(Guid libraryId)
    {
        var items = new List<BaseItem>();

        var library = _libraryManager.GetItemById(libraryId);
        if (library == null)
        {
            _logger.LogWarning("Library {LibraryId} not found", libraryId);
            return items;
        }

        // Get all movies and TV episodes from the library
        var query = new InternalItemsQuery
        {
            IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Episode },
            Recursive = true,
            ParentId = libraryId
        };

        items = _libraryManager.GetItemList(query).ToList();

        return items;
    }

    /// <summary>
    /// Groups items by normalized title.
    /// </summary>
    private Dictionary<string, List<BaseItem>> GroupByNormalizedTitle(List<BaseItem> items)
    {
        var groups = new Dictionary<string, List<BaseItem>>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            var normalizedTitle = NormalizeTitle(item.Name);
            if (string.IsNullOrWhiteSpace(normalizedTitle))
            {
                continue;
            }

            if (!groups.TryGetValue(normalizedTitle, out var group))
            {
                group = new List<BaseItem>();
                groups[normalizedTitle] = group;
            }

            group.Add(item);
        }

        return groups;
    }

    /// <summary>
    /// Normalizes a title for comparison.
    /// </summary>
    private string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        // Convert to lowercase
        var normalized = title.ToLowerInvariant();

        // Remove special characters and extra whitespace
        normalized = Regex.Replace(normalized, @"[^\w\s]", string.Empty);
        normalized = Regex.Replace(normalized, @"\s+", " ");

        return normalized.Trim();
    }

    /// <summary>
    /// Detects duplicates within a group of items with similar titles.
    /// </summary>
    private DuplicateGroup? DetectDuplicatesInGroup(List<BaseItem> items, LibraryPreferences preferences)
    {
        if (items.Count < 2)
        {
            return null;
        }

        var duplicateVersions = new List<BaseItem>();

        // Compare all items pairwise to find duplicates
        for (int i = 0; i < items.Count; i++)
        {
            for (int j = i + 1; j < items.Count; j++)
            {
                var score = CalculateSimilarityScore(items[i], items[j]);
                if (score >= preferences.SimilarityThreshold)
                {
                    if (!duplicateVersions.Contains(items[i]))
                    {
                        duplicateVersions.Add(items[i]);
                    }

                    if (!duplicateVersions.Contains(items[j]))
                    {
                        duplicateVersions.Add(items[j]);
                    }
                }
            }
        }

        if (duplicateVersions.Count < 2)
        {
            return null;
        }

        // Create duplicate group
        var duplicateGroup = new DuplicateGroup();

        // Convert items to version records (basic info for now, quality analysis comes later)
        foreach (var item in duplicateVersions)
        {
            var version = new VersionRecord
            {
                ItemId = item.Id,
                FilePath = item.Path ?? string.Empty,
                FileSize = GetFileSize(item)
            };

            duplicateGroup.Versions.Add(version);
        }

        // Set the first version as primary by default
        if (duplicateGroup.Versions.Count > 0)
        {
            duplicateGroup.PrimaryVersionId = duplicateGroup.Versions[0].ItemId;
        }

        return duplicateGroup;
    }

    /// <summary>
    /// Calculates similarity score between two items.
    /// </summary>
    private int CalculateSimilarityScore(BaseItem item1, BaseItem item2)
    {
        int score = 0;

        // Title exact match: 30 points
        if (string.Equals(
            NormalizeTitle(item1.Name),
            NormalizeTitle(item2.Name),
            StringComparison.OrdinalIgnoreCase))
        {
            score += 30;
        }

        // Year match: 20 points (with ±1 year tolerance)
        if (item1.ProductionYear.HasValue && item2.ProductionYear.HasValue)
        {
            var yearDiff = Math.Abs(item1.ProductionYear.Value - item2.ProductionYear.Value);
            if (yearDiff == 0)
            {
                score += 20;
            }
            else if (yearDiff == 1)
            {
                score += 10; // Partial credit for adjacent years
            }
        }

        // IMDb ID match: 40 points
        var imdbId1 = item1.ProviderIds.GetValueOrDefault("Imdb");
        var imdbId2 = item2.ProviderIds.GetValueOrDefault("Imdb");
        if (!string.IsNullOrEmpty(imdbId1) && !string.IsNullOrEmpty(imdbId2) &&
            string.Equals(imdbId1, imdbId2, StringComparison.OrdinalIgnoreCase))
        {
            score += 40;
        }

        // TMDb ID match: 40 points
        var tmdbId1 = item1.ProviderIds.GetValueOrDefault("Tmdb");
        var tmdbId2 = item2.ProviderIds.GetValueOrDefault("Tmdb");
        if (!string.IsNullOrEmpty(tmdbId1) && !string.IsNullOrEmpty(tmdbId2) &&
            string.Equals(tmdbId1, tmdbId2, StringComparison.OrdinalIgnoreCase))
        {
            score += 40;
        }

        // Runtime similarity (within 5 minutes): 10 points
        if (item1.RunTimeTicks.HasValue && item2.RunTimeTicks.HasValue)
        {
            var runtime1 = TimeSpan.FromTicks(item1.RunTimeTicks.Value).TotalMinutes;
            var runtime2 = TimeSpan.FromTicks(item2.RunTimeTicks.Value).TotalMinutes;
            var runtimeDiff = Math.Abs(runtime1 - runtime2);
            if (runtimeDiff <= 5)
            {
                score += 10;
            }
        }

        return score;
    }

    /// <summary>
    /// Gets the file size for an item.
    /// </summary>
    private long GetFileSize(BaseItem item)
    {
        try
        {
            if (!string.IsNullOrEmpty(item.Path) && System.IO.File.Exists(item.Path))
            {
                return new System.IO.FileInfo(item.Path).Length;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting file size for {Path}", item.Path);
        }

        return 0;
    }
}
