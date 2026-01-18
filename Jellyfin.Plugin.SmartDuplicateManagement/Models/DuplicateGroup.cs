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
    /// Gets or sets the union of all unique genres.
    /// </summary>
    public IList<string> Genres { get; set; }

    /// <summary>
    /// Gets or sets the union of all unique tags.
    /// </summary>
    public IList<string> Tags { get; set; }

    /// <summary>
    /// Gets or sets the union of all people (actors, directors, etc.).
    /// </summary>
    public IList<string> People { get; set; }

    /// <summary>
    /// Gets or sets the average of all ratings.
    /// </summary>
    public double AverageRating { get; set; }

    /// <summary>
    /// Gets or sets the earliest release date.
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Gets or sets the union of all studios.
    /// </summary>
    public IList<string> Studios { get; set; }

    /// <summary>
    /// Gets or sets the union of all external IDs.
    /// </summary>
    public IDictionary<string, string> ExternalIds { get; set; }

    /// <summary>
    /// Gets or sets the list of unique descriptions.
    /// </summary>
    public IList<string> Descriptions { get; set; }
}

/// <summary>
/// Represents a group of duplicate media items.
/// </summary>
public class DuplicateGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateGroup"/> class.
    /// </summary>
    public DuplicateGroup()
    {
        GroupId = Guid.NewGuid();
        Versions = new List<VersionRecord>();
        MergedMetadata = new MergedMetadata();
        DetectionTimestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets or sets the unique identifier for this duplicate group.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Gets or sets the library containing these duplicates.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the item ID of the primary version.
    /// </summary>
    public Guid PrimaryVersionId { get; set; }

    /// <summary>
    /// Gets or sets the collection of all version records.
    /// </summary>
    public IList<VersionRecord> Versions { get; set; }

    /// <summary>
    /// Gets or sets the combined metadata from all versions.
    /// </summary>
    public MergedMetadata MergedMetadata { get; set; }

    /// <summary>
    /// Gets or sets when this group was identified.
    /// </summary>
    public DateTime DetectionTimestamp { get; set; }

    /// <summary>
    /// Gets or sets when user last reviewed this group.
    /// </summary>
    public DateTime? LastReviewedTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the review status of this group.
    /// </summary>
    public string Status { get; set; } = "Pending";
}
