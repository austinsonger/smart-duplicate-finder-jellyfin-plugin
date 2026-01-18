using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Models;

/// <summary>
/// Represents a single version of a media item within a duplicate group.
/// </summary>
public class VersionRecord
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionRecord"/> class.
    /// </summary>
    public VersionRecord()
    {
        MetadataContribution = new List<string>();
    }

    /// <summary>
    /// Gets or sets the Jellyfin BaseItem identifier.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the full path to the media file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the calculated quality rank (higher is better).
    /// </summary>
    public int QualityScore { get; set; }

    /// <summary>
    /// Gets or sets the video resolution (e.g., "3840x2160").
    /// </summary>
    public string Resolution { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video codec (e.g., "HEVC", "H264").
    /// </summary>
    public string Codec { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dynamic range (HDR or SDR).
    /// </summary>
    public string DynamicRange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary audio codec.
    /// </summary>
    public string AudioCodec { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the audio configuration (e.g., "7.1").
    /// </summary>
    public string AudioChannels { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source classification (e.g., "Remux", "WEB-DL", "BluRay").
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the overall bitrate in kbps.
    /// </summary>
    public int Bitrate { get; set; }

    /// <summary>
    /// Gets the metadata fields originating from this version.
    /// </summary>
    public IList<string> MetadataContribution { get; }
}
