using System.Collections.Generic;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Configuration;

/// <summary>
/// Library-specific preferences for duplicate detection and quality management.
/// </summary>
public class LibraryPreferences
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPreferences"/> class.
    /// </summary>
    public LibraryPreferences()
    {
        // Default resolution priority (highest to lowest)
        ResolutionPriority = new List<string> { "4320p", "2160p", "1440p", "1080p", "720p", "576p", "480p" };

        // Default dynamic range priority
        DynamicRangePriority = new List<string> { "HDR10+", "Dolby Vision", "HDR10", "HLG", "SDR" };

        // Default codec priority
        CodecPriority = new List<string> { "AV1", "HEVC", "H.264", "VP9", "MPEG-4" };

        // Default audio format priority
        AudioPriority = new List<string> { "Dolby Atmos", "DTS:X", "TrueHD 7.1", "DTS-HD MA 7.1", "DTS-HD MA 5.1", "AC3 5.1", "AAC Stereo" };

        // Default source type priority
        SourceTypePriority = new List<string> { "Remux", "BluRay", "WEB-DL", "WEBRip", "HDTV", "DVDRip" };

        AutoDeleteEnabled = false;
        MinimumQualityThreshold = string.Empty;
        RequireManualReview = true;
        SimilarityThreshold = 50;
    }

    /// <summary>
    /// Gets or sets the library identifier.
    /// </summary>
    public string LibraryId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the ordered list of resolution preferences.
    /// </summary>
    public IList<string> ResolutionPriority { get; }

    /// <summary>
    /// Gets the ordered list of codec preferences.
    /// </summary>
    public IList<string> CodecPriority { get; }

    /// <summary>
    /// Gets the ordered list of dynamic range preferences.
    /// </summary>
    public IList<string> DynamicRangePriority { get; }

    /// <summary>
    /// Gets the ordered list of audio format preferences.
    /// </summary>
    public IList<string> AudioPriority { get; }

    /// <summary>
    /// Gets the ordered list of source type preferences.
    /// </summary>
    public IList<string> SourceTypePriority { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically delete lower-quality duplicates.
    /// </summary>
    public bool AutoDeleteEnabled { get; set; }

    /// <summary>
    /// Gets or sets the minimum quality threshold for retention (e.g., "4K", "1080p HDR").
    /// </summary>
    public string MinimumQualityThreshold { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to require user confirmation before deletion.
    /// </summary>
    public bool RequireManualReview { get; set; }

    /// <summary>
    /// Gets or sets the similarity threshold for duplicate detection (0-100).
    /// </summary>
    public int SimilarityThreshold { get; set; }
}
