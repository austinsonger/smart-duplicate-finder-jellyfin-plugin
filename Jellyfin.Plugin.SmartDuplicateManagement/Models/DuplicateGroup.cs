using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Models;

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
    /// Gets the collection of all version records.
    /// </summary>
    public IList<VersionRecord> Versions { get; }

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
