using System;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Models;

/// <summary>
/// Represents a deletion audit log entry.
/// </summary>
public class DeletionAuditRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for this audit record.
    /// </summary>
    public Guid RecordId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the duplicate group identifier.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Gets or sets the deleted item identifier.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the file path that was deleted.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quality score of the deleted version.
    /// </summary>
    public int QualityScore { get; set; }

    /// <summary>
    /// Gets or sets the reason for deletion.
    /// </summary>
    public string DeletionReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether deletion was user-initiated.
    /// </summary>
    public bool UserInitiated { get; set; }

    /// <summary>
    /// Gets or sets the user ID who initiated the deletion (if applicable).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when deletion occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the deletion was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if deletion failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
