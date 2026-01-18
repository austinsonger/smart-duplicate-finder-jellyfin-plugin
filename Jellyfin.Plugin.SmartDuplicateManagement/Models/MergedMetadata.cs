using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Models;

/// <summary>
/// Represents merged metadata from all versions of a duplicate group.
/// </summary>
public class MergedMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MergedMetadata"/> class.
    /// </summary>
    public MergedMetadata()
    {
        Genres = new List<string>();
        Tags = new List<string>();
        People = new List<string>();
        Studios = new List<string>();
        ExternalIds = new Dictionary<string, string>();
        Descriptions = new List<string>();
    }

    /// <summary>
    /// Gets or sets the title (most complete version).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets the union of all unique genres.
    /// </summary>
    public IList<string> Genres { get; }

    /// <summary>
    /// Gets the union of all unique tags.
    /// </summary>
    public IList<string> Tags { get; }

    /// <summary>
    /// Gets the union of all people (actors, directors, etc.).
    /// </summary>
    public IList<string> People { get; }

    /// <summary>
    /// Gets or sets the average of all ratings.
    /// </summary>
    public double AverageRating { get; set; }

    /// <summary>
    /// Gets or sets the earliest release date.
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Gets the union of all studios.
    /// </summary>
    public IList<string> Studios { get; }

    /// <summary>
    /// Gets the union of all external IDs.
    /// </summary>
    public IDictionary<string, string> ExternalIds { get; }

    /// <summary>
    /// Gets the list of unique descriptions.
    /// </summary>
    public IList<string> Descriptions { get; }
}
