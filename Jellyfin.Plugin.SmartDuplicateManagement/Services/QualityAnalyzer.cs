using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.SmartDuplicateManagement.Configuration;
using Jellyfin.Plugin.SmartDuplicateManagement.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Services;

/// <summary>
/// Service for analyzing media quality and calculating quality scores.
/// </summary>
public class QualityAnalyzer
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<QualityAnalyzer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QualityAnalyzer"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    public QualityAnalyzer(
        ILibraryManager libraryManager,
        ILogger<QualityAnalyzer> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes and scores all versions in a duplicate group.
    /// </summary>
    /// <param name="group">The duplicate group.</param>
    /// <param name="preferences">The library preferences.</param>
    public void AnalyzeQuality(DuplicateGroup group, LibraryPreferences preferences)
    {
        foreach (var version in group.Versions)
        {
            var item = _libraryManager.GetItemById(version.ItemId);
            if (item == null)
            {
                _logger.LogWarning("Item {ItemId} not found", version.ItemId);
                continue;
            }

            ExtractMediaInfo(version, item);
            version.QualityScore = CalculateQualityScore(version, preferences);
        }

        // Sort versions by quality score (highest first)
        group.Versions = group.Versions.OrderByDescending(v => v.QualityScore).ToList();

        // Set highest quality as primary if not already set
        if (group.Versions.Count > 0 && group.PrimaryVersionId == Guid.Empty)
        {
            group.PrimaryVersionId = group.Versions[0].ItemId;
        }
    }

    /// <summary>
    /// Extracts media information from a BaseItem.
    /// </summary>
    private void ExtractMediaInfo(VersionRecord version, BaseItem item)
    {
        try
        {
            // Get video stream info
            var mediaStreams = item.GetMediaStreams();
            var videoStream = mediaStreams.FirstOrDefault(s => s.Type == MediaStreamType.Video);
            var audioStream = mediaStreams.FirstOrDefault(s => s.Type == MediaStreamType.Audio);

            if (videoStream != null)
            {
                // Resolution
                if (videoStream.Width.HasValue && videoStream.Height.HasValue)
                {
                    version.Resolution = $"{videoStream.Width}x{videoStream.Height}";
                }

                // Codec
                version.Codec = videoStream.Codec ?? string.Empty;

                // Dynamic Range
                version.DynamicRange = DetectDynamicRange(videoStream);

                // Bitrate
                if (videoStream.BitRate.HasValue)
                {
                    version.Bitrate = videoStream.BitRate.Value / 1000; // Convert to kbps
                }
            }

            if (audioStream != null)
            {
                // Audio codec
                version.AudioCodec = audioStream.Codec ?? string.Empty;

                // Audio channels
                if (audioStream.Channels.HasValue)
                {
                    version.AudioChannels = FormatAudioChannels(audioStream.Channels.Value);
                }
            }

            // Infer source type from filename
            version.SourceType = InferSourceType(version.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting media info for {ItemId}", version.ItemId);
        }
    }

    /// <summary>
    /// Detects dynamic range from video stream.
    /// </summary>
    private string DetectDynamicRange(MediaStream stream)
    {
        // Check for Dolby Vision or HDR10+ in codec profile
        var profile = stream.Profile ?? string.Empty;
        if (profile.Contains("Dolby Vision", StringComparison.OrdinalIgnoreCase))
        {
            return "Dolby Vision";
        }

        if (profile.Contains("HDR10+", StringComparison.OrdinalIgnoreCase) ||
            profile.Contains("HDR10Plus", StringComparison.OrdinalIgnoreCase))
        {
            return "HDR10+";
        }

        // Check video range string
        var videoRange = stream.VideoRangeType.ToString();
        if (videoRange.Contains("HDR", StringComparison.OrdinalIgnoreCase))
        {
            return "HDR10";
        }

        if (videoRange.Contains("HLG", StringComparison.OrdinalIgnoreCase))
        {
            return "HLG";
        }

        return "SDR";
    }

    /// <summary>
    /// Formats audio channel count to standard notation.
    /// </summary>
    private string FormatAudioChannels(int channels)
    {
        return channels switch
        {
            8 => "7.1",
            6 => "5.1",
            2 => "Stereo",
            1 => "Mono",
            _ => channels.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Infers source type from filename patterns.
    /// </summary>
    private string InferSourceType(string filePath)
    {
        var fileName = Path.GetFileName(filePath).ToUpperInvariant();

        if (fileName.Contains("REMUX", StringComparison.Ordinal))
        {
            return "Remux";
        }

        if (fileName.Contains("BLURAY", StringComparison.Ordinal) || fileName.Contains("BLU-RAY", StringComparison.Ordinal))
        {
            return "BluRay";
        }

        if (fileName.Contains("WEB-DL", StringComparison.Ordinal) || fileName.Contains("WEBDL", StringComparison.Ordinal))
        {
            return "WEB-DL";
        }

        if (fileName.Contains("WEBRIP", StringComparison.Ordinal))
        {
            return "WEBRip";
        }

        if (fileName.Contains("HDTV", StringComparison.Ordinal))
        {
            return "HDTV";
        }

        if (fileName.Contains("DVDRIP", StringComparison.Ordinal) || fileName.Contains("DVD-RIP", StringComparison.Ordinal))
        {
            return "DVDRip";
        }

        return "Unknown";
    }

    /// <summary>
    /// Calculates quality score based on preferences.
    /// </summary>
    private int CalculateQualityScore(VersionRecord version, LibraryPreferences preferences)
    {
        double score = 0;

        // Resolution score (30% weight)
        var resolutionScore = GetPriorityScore(
            GetResolutionCategory(version.Resolution),
            preferences.ResolutionPriority);
        score += resolutionScore * 0.30;

        // Dynamic Range score (25% weight)
        var dynamicRangeScore = GetPriorityScore(
            version.DynamicRange,
            preferences.DynamicRangePriority);
        score += dynamicRangeScore * 0.25;

        // Codec score (20% weight)
        var codecScore = GetPriorityScore(
            NormalizeCodec(version.Codec),
            preferences.CodecPriority);
        score += codecScore * 0.20;

        // Audio score (15% weight)
        var audioScore = GetPriorityScore(
            GetAudioFormat(version.AudioCodec, version.AudioChannels),
            preferences.AudioPriority);
        score += audioScore * 0.15;

        // Source Type score (10% weight)
        var sourceScore = GetPriorityScore(
            version.SourceType,
            preferences.SourceTypePriority);
        score += sourceScore * 0.10;

        return (int)Math.Round(score);
    }

    /// <summary>
    /// Gets the priority score for an item in a list.
    /// </summary>
    private int GetPriorityScore(string item, IList<string> priorityList)
    {
        if (string.IsNullOrEmpty(item) || priorityList.Count == 0)
        {
            return 0;
        }

        var index = -1;
        for (int i = 0; i < priorityList.Count; i++)
        {
            if (string.Equals(priorityList[i], item, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            return 0;
        }

        return (int)Math.Round(((double)(priorityList.Count - index) / priorityList.Count) * 100);
    }

    /// <summary>
    /// Gets resolution category from resolution string.
    /// </summary>
    private string GetResolutionCategory(string resolution)
    {
        if (string.IsNullOrEmpty(resolution))
        {
            return string.Empty;
        }

        // Parse height from resolution string
        var match = Regex.Match(resolution, @"x(\d+)");
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out int height))
        {
            return string.Empty;
        }

        return height switch
        {
            >= 2160 => "2160p",
            >= 1440 => "1440p",
            >= 1080 => "1080p",
            >= 720 => "720p",
            >= 576 => "576p",
            >= 480 => "480p",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Normalizes codec name for comparison.
    /// </summary>
    private string NormalizeCodec(string codec)
    {
        if (string.IsNullOrEmpty(codec))
        {
            return string.Empty;
        }

        codec = codec.ToUpperInvariant();

        if (codec.Contains("HEVC", StringComparison.Ordinal) || codec.Contains("H265", StringComparison.Ordinal) || codec.Contains("H.265", StringComparison.Ordinal))
        {
            return "HEVC";
        }

        if (codec.Contains("H264", StringComparison.Ordinal) || codec.Contains("H.264", StringComparison.Ordinal) || codec.Contains("AVC", StringComparison.Ordinal))
        {
            return "H.264";
        }

        if (codec.Contains("AV1", StringComparison.Ordinal))
        {
            return "AV1";
        }

        if (codec.Contains("VP9", StringComparison.Ordinal))
        {
            return "VP9";
        }

        if (codec.Contains("MPEG", StringComparison.Ordinal))
        {
            return "MPEG-4";
        }

        return codec;
    }

    /// <summary>
    /// Gets audio format description.
    /// </summary>
    private string GetAudioFormat(string codec, string channels)
    {
        if (string.IsNullOrEmpty(codec))
        {
            return string.Empty;
        }

        codec = codec.ToUpperInvariant();

        // Check for premium formats
        if (codec.Contains("ATMOS", StringComparison.Ordinal))
        {
            return "Dolby Atmos";
        }

        if (codec.Contains("DTS:X", StringComparison.Ordinal) || codec.Contains("DTSX", StringComparison.Ordinal))
        {
            return "DTS:X";
        }

        if (codec.Contains("TRUEHD", StringComparison.Ordinal))
        {
            return channels == "7.1" ? "TrueHD 7.1" : "TrueHD 5.1";
        }

        if (codec.Contains("DTS-HD", StringComparison.Ordinal) || codec.Contains("DTSHD", StringComparison.Ordinal))
        {
            return channels == "7.1" ? "DTS-HD MA 7.1" : "DTS-HD MA 5.1";
        }

        if (codec.Contains("AC3", StringComparison.Ordinal) || codec.Contains("DD", StringComparison.Ordinal))
        {
            return "AC3 5.1";
        }

        if (codec.Contains("AAC", StringComparison.Ordinal))
        {
            return "AAC Stereo";
        }

        return $"{codec} {channels}";
    }
}
