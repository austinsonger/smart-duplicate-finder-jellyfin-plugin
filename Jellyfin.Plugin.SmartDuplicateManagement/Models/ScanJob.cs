using System;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Models;

/// <summary>
/// Represents a scan job status.
/// </summary>
public class ScanJob
{
    /// <summary>
    /// Gets or sets the unique job identifier.
    /// </summary>
    public Guid JobId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the library identifier being scanned.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the job status.
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets the progress percentage (0-100).
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the job started.
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the job completed.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the number of duplicates found.
    /// </summary>
    public int DuplicatesFound { get; set; }

    /// <summary>
    /// Gets or sets the total items processed.
    /// </summary>
    public int ItemsProcessed { get; set; }
}
