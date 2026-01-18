using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.SmartDuplicateManagement.Models;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SmartDuplicateManagement.Services;

/// <summary>
/// Service for persisting duplicate groups and audit logs.
/// </summary>
public class DataPersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly IApplicationPaths _applicationPaths;
    private readonly ILogger<DataPersistenceService> _logger;
    private readonly string _dataDirectory;
    private readonly string _auditDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPersistenceService"/> class.
    /// </summary>
    /// <param name="applicationPaths">The application paths.</param>
    /// <param name="logger">The logger.</param>
    public DataPersistenceService(
        IApplicationPaths applicationPaths,
        ILogger<DataPersistenceService> logger)
    {
        _applicationPaths = applicationPaths;
        _logger = logger;

        var pluginDataPath = Path.Combine(applicationPaths.PluginsPath, "SmartDuplicateManagement");
        _dataDirectory = Path.Combine(pluginDataPath, "data");
        _auditDirectory = Path.Combine(pluginDataPath, "audit");

        EnsureDirectoriesExist();
    }

    /// <summary>
    /// Ensures that data directories exist.
    /// </summary>
    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_dataDirectory);
        Directory.CreateDirectory(_auditDirectory);
    }

    /// <summary>
    /// Saves duplicate groups for a library.
    /// </summary>
    /// <param name="libraryId">The library identifier.</param>
    /// <param name="groups">The duplicate groups.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous save operation.</returns>
    public async Task SaveDuplicateGroupsAsync(Guid libraryId, IReadOnlyCollection<DuplicateGroup> groups)
    {
        try
        {
            var filePath = Path.Combine(_dataDirectory, $"duplicates_{libraryId}.json");
            var json = JsonSerializer.Serialize(groups, JsonOptions);
            await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
            _logger.LogInformation("Saved {Count} duplicate groups for library {LibraryId}", groups.Count, libraryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving duplicate groups for library {LibraryId}", libraryId);
        }
    }

    /// <summary>
    /// Loads duplicate groups for a library.
    /// </summary>
    /// <param name="libraryId">The library identifier.</param>
    /// <returns>List of duplicate groups.</returns>
    public async Task<List<DuplicateGroup>> LoadDuplicateGroupsAsync(Guid libraryId)
    {
        try
        {
            var filePath = Path.Combine(_dataDirectory, $"duplicates_{libraryId}.json");
            if (!File.Exists(filePath))
            {
                return new List<DuplicateGroup>();
            }

            var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            var groups = JsonSerializer.Deserialize<List<DuplicateGroup>>(json);
            return groups ?? new List<DuplicateGroup>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading duplicate groups for library {LibraryId}", libraryId);
            return new List<DuplicateGroup>();
        }
    }

    /// <summary>
    /// Logs a deletion audit record.
    /// </summary>
    /// <param name="record">The audit record.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous logging operation.</returns>
    public async Task LogDeletionAsync(DeletionAuditRecord record)
    {
        try
        {
            var fileName = $"audit_{DateTime.UtcNow:yyyy_MM}.jsonl";
            var filePath = Path.Combine(_auditDirectory, fileName);

            var json = JsonSerializer.Serialize(record);
            await File.AppendAllLinesAsync(filePath, new[] { json }).ConfigureAwait(false);

            _logger.LogInformation("Logged deletion audit for item {ItemId}", record.ItemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging deletion audit");
        }
    }

    /// <summary>
    /// Gets deletion audit records.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>List of audit records.</returns>
    public async Task<List<DeletionAuditRecord>> GetAuditRecordsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var records = new List<DeletionAuditRecord>();

        try
        {
            var files = Directory.GetFiles(_auditDirectory, "audit_*.jsonl");

            foreach (var file in files)
            {
                var lines = await File.ReadAllLinesAsync(file).ConfigureAwait(false);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    try
                    {
                        var record = JsonSerializer.Deserialize<DeletionAuditRecord>(line);
                        if (record != null)
                        {
                            if ((!startDate.HasValue || record.Timestamp >= startDate.Value) &&
                                (!endDate.HasValue || record.Timestamp <= endDate.Value))
                            {
                                records.Add(record);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deserializing audit record");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit records");
        }

        return records.OrderByDescending(r => r.Timestamp).ToList();
    }
}
