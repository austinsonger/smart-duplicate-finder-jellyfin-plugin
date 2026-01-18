using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.SmartDuplicateManagement.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Services;

/// <summary>
/// Service for merging metadata from multiple versions.
/// </summary>
public class MetadataMerger
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<MetadataMerger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataMerger"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    public MetadataMerger(
        ILibraryManager libraryManager,
        ILogger<MetadataMerger> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    /// <summary>
    /// Merges metadata from all versions in a duplicate group.
    /// </summary>
    /// <param name="group">The duplicate group.</param>
    public void MergeMetadata(DuplicateGroup group)
    {
        try
        {
            var items = new List<BaseItem>();
            foreach (var version in group.Versions)
            {
                var item = _libraryManager.GetItemById(version.ItemId);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            if (items.Count == 0)
            {
                _logger.LogWarning("No items found for duplicate group {GroupId}", group.GroupId);
                return;
            }

            var merged = group.MergedMetadata;

            // Title: Use most complete version (longest)
            merged.Title = items
                .Select(i => i.Name)
                .Where(n => !string.IsNullOrEmpty(n))
                .OrderByDescending(n => n.Length)
                .FirstOrDefault() ?? string.Empty;

            // Genres: Union of all unique genres
            var genres = items
                .SelectMany(i => i.Genres ?? Enumerable.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            merged.Genres.Clear();
            foreach (var genre in genres)
            {
                merged.Genres.Add(genre);
            }

            // Tags: Union of all unique tags
            var tags = items
                .SelectMany(i => i.Tags ?? Enumerable.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            merged.Tags.Clear();
            foreach (var tag in tags)
            {
                merged.Tags.Add(tag);
            }

            // People: Union of all unique people (using GetPeople method)
            var allPeople = new List<string>();
            foreach (var item in items)
            {
                var peopleInfo = _libraryManager.GetPeople(item);
                allPeople.AddRange(peopleInfo.Select(p => p.Name));
            }

            var people = allPeople
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            merged.People.Clear();
            foreach (var person in people)
            {
                merged.People.Add(person);
            }

            // Ratings: Average of all ratings
            var ratings = items
                .Where(i => i.CommunityRating.HasValue)
                .Select(i => i.CommunityRating!.Value)
                .ToList();
            merged.AverageRating = ratings.Count > 0 ? ratings.Average() : 0;

            // Release Date: Use earliest date
            var dates = items
                .Select(i => i.PremiereDate)
                .Where(d => d.HasValue)
                .OrderBy(d => d!.Value)
                .ToList();
            merged.ReleaseDate = dates.FirstOrDefault();

            // Studios: Union of all studios
            var studios = items
                .SelectMany(i => i.Studios ?? Enumerable.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            merged.Studios.Clear();
            foreach (var studio in studios)
            {
                merged.Studios.Add(studio);
            }

            // External IDs: Union of all IDs
            merged.ExternalIds.Clear();
            foreach (var item in items)
            {
                foreach (var kvp in item.ProviderIds)
                {
                    if (!merged.ExternalIds.ContainsKey(kvp.Key))
                    {
                        merged.ExternalIds[kvp.Key] = kvp.Value;
                    }
                }
            }

            // Descriptions: Collect unique descriptions
            var descriptions = items
                .Select(i => i.Overview)
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            merged.Descriptions.Clear();
            foreach (var desc in descriptions)
            {
                if (desc != null)
                {
                    merged.Descriptions.Add(desc);
                }
            }

            _logger.LogInformation("Merged metadata for duplicate group {GroupId}", group.GroupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging metadata for group {GroupId}", group.GroupId);
        }
    }
}
