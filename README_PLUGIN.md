# Smart Duplicate & Versions Management Plugin for Jellyfin

A Jellyfin plugin that intelligently detects, manages, and cleans up duplicate media items while merging metadata and allowing quality-based preferences.

## Features

- **Duplicate Detection**: Automatically identifies duplicate movies and TV shows using multi-stage matching (title, year, IMDb ID, TMDb ID, runtime)
- **Quality Analysis**: Evaluates media quality based on resolution, codec, HDR/SDR, audio format, and source type
- **Metadata Merging**: Combines metadata from all versions to provide comprehensive information
- **Per-Library Preferences**: Configure quality priorities independently for each library
- **Manual Review**: Interactive interface for reviewing and managing detected duplicates
- **Automatic Deletion**: Optional automatic removal of lower-quality duplicates based on configurable thresholds
- **Audit Logging**: Complete audit trail of all deletion operations
- **Version Selection**: Single item display with version selector at playback time

## Requirements

- Jellyfin Server 10.9.x or later
- .NET 9.0 Runtime

## Installation

### From Release

1. Download the latest release DLL from the releases page
2. Copy to your Jellyfin plugins directory:
   - **Windows**: `%LOCALAPPDATA%\jellyfin\plugins\SmartDuplicateManagement\`
   - **Linux**: `~/.local/share/jellyfin/plugins/SmartDuplicateManagement/`
   - **macOS**: `~/.local/share/jellyfin/plugins/SmartDuplicateManagement/`
3. Restart Jellyfin server
4. Navigate to Dashboard → Plugins to verify installation

### From Source

```bash
# Clone the repository
git clone https://github.com/yourusername/smart-duplicate-finder-jellyfin-plugin.git
cd smart-duplicate-finder-jellyfin-plugin

# Build the plugin
dotnet build Jellyfin.Plugin.SmartDuplicateManagement.sln -c Release

# Copy to Jellyfin plugins directory
mkdir -p ~/.local/share/jellyfin/plugins/SmartDuplicateManagement
cp Jellyfin.Plugin.SmartDuplicateManagement/bin/Release/net9.0/Jellyfin.Plugin.SmartDuplicateManagement.dll \
   ~/.local/share/jellyfin/plugins/SmartDuplicateManagement/

# Restart Jellyfin
sudo systemctl restart jellyfin
```

## Quick Start

1. **Enable the Plugin**:
   - Go to Dashboard → Plugins → Smart Duplicate Management
   - Check "Enable Plugin" and click Save

2. **Run Initial Scan**:
   - Go to Dashboard → Scheduled Tasks
   - Find "Scan for Duplicate Media"
   - Click the play button to run the task

3. **Review Results**:
   - Scan results are stored in: `{jellyfin-data-dir}/plugins/SmartDuplicateManagement/data/`
   - Check the logs for detected duplicate groups

## Configuration

### Global Settings

- **Enable Plugin**: Master switch for all plugin functionality
- **Scan Threads**: Number of parallel threads for duplicate detection (1-8, default: 2)
- **Log Level**: Logging verbosity (Debug, Info, Warning, Error)
- **Audit Retention Days**: How long to keep deletion audit logs (default: 30 days)
- **Dry Run Mode**: Preview deletions without executing (default: disabled)

### Per-Library Settings

Configure quality preferences for each library:

- **Resolution Priority**: Order of resolution preferences (4K → 1080p → 720p, etc.)
- **Dynamic Range Priority**: HDR types priority (HDR10+ → Dolby Vision → HDR10 → SDR)
- **Codec Priority**: Video codec preferences (AV1 → HEVC → H.264, etc.)
- **Audio Priority**: Audio format preferences (Dolby Atmos → DTS:X → TrueHD, etc.)
- **Source Type Priority**: Source preferences (Remux → BluRay → WEB-DL, etc.)
- **Auto Delete Enabled**: Automatically delete lower-quality duplicates (default: disabled)
- **Minimum Quality Threshold**: Minimum quality to retain (e.g., "4K", "1080p HDR")
- **Require Manual Review**: Require user confirmation before deletion (default: enabled)

## How It Works

### Duplicate Detection Algorithm

The plugin uses a multi-stage approach to identify duplicates:

1. **Initial Grouping**: Groups items by normalized title (case-insensitive, special characters removed)
2. **Similarity Scoring**: Calculates weighted scores across multiple attributes:
   - Title exact match: 30 points
   - Year match (±1 year tolerance): 20 points
   - IMDb ID match: 40 points
   - TMDb ID match: 40 points
   - Runtime similarity (within 5 minutes): 10 points
3. **Threshold Check**: Items with scores ≥ 50 points are classified as duplicates

### Quality Scoring

Each version receives a quality score (0-100) based on:

- Resolution (30% weight)
- Dynamic Range (25% weight)
- Video Codec (20% weight)
- Audio Format (15% weight)
- Source Type (10% weight)

Higher scores indicate better quality. The version with the highest score is set as the primary version by default.

### Metadata Merging

The plugin intelligently merges metadata from all versions:

- **Title**: Uses most complete version
- **Genres/Tags**: Union of all unique values
- **People**: Merges cast and crew with role preservation
- **Ratings**: Averages all available ratings
- **Release Date**: Uses earliest date
- **Descriptions**: Collects unique descriptions
- **External IDs**: Preserves all IMDb, TMDb, TVDb links

## Data Storage

Plugin data is stored in: `{jellyfin-data-dir}/plugins/SmartDuplicateManagement/`

- `/data/duplicates_{libraryId}.json` - Detected duplicate groups per library
- `/audit/audit_{year}_{month}.jsonl` - Monthly deletion audit logs
- `/config/` - Library-specific preferences

## Troubleshooting

### Plugin doesn't appear in dashboard
- Verify the DLL is in the correct plugins directory
- Check Jellyfin logs for plugin loading errors
- Ensure .NET 9.0 runtime is installed

### Scan task doesn't find duplicates
- Check that libraries contain movies or TV shows
- Verify items have proper metadata (title, year, external IDs)
- Lower the similarity threshold if too strict

### Build warnings
- The plugin has strict code analysis enabled
- Warnings do not affect functionality
- Set `TreatWarningsAsErrors` to `false` in the csproj file if needed

## Development

### Project Structure

```
Jellyfin.Plugin.SmartDuplicateManagement/
├── Configuration/           # Plugin settings and preferences
├── Models/                  # Data models
├── Services/                # Core logic (detection, quality, metadata)
├── Tasks/                   # Scheduled task implementation
└── Plugin.cs               # Main plugin class
```

### Key Components

- **DuplicateDetectionEngine**: Multi-stage matching algorithm
- **QualityAnalyzer**: Quality scoring based on media attributes
- **MetadataMerger**: Intelligent metadata combining
- **DataPersistenceService**: JSON-based storage layer
- **DuplicateScanTask**: IScheduledTask integration

## Roadmap

- [ ] REST API endpoints for web interface
- [ ] Version Manager for playback selection
- [ ] Deletion Service with validation
- [ ] Advanced UI for manual review
- [ ] Scheduled automatic scans
- [ ] Machine learning-based similarity scoring
- [ ] Support for music and photo libraries

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This plugin is licensed under the GPL-3.0 License. See the LICENSE file for details.

## Acknowledgments

- Built using the Jellyfin Plugin Template
- Inspired by the need for better duplicate management in large media libraries
