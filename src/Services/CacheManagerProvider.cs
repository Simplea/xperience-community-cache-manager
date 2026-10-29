using System.Data;
using System.Security.Cryptography;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.Helpers.Caching.Abstractions;

using XperienceCommunity.CacheManager.Models;

namespace XperienceCommunity.CacheManager.Services;

/// <summary>
/// Default implementation of <see cref="ICacheManagerProvider"/>.
/// </summary>
public class CacheManagerProvider(IEventLogService eventLogService, ICacheAccessor cacheAccessor) : ICacheManagerProvider
{
    private DataSet? result;
    public const string ROW_IDENTIFIER_COLUMN = $"{nameof(CacheManagerProvider)}_result_identifier";
    public DataSet? Get()
    {
        EnsureResult();

        if (ResultsAreEmpty())
        {
            return null;
        }

        return result;
    }

    public void Clear() => CacheHelper.ClearCache();

    public int Count()
    {
        EnsureResult();

        if (ResultsAreEmpty())
        {
            return 0;
        }

        return result!.Tables[0].Rows.Count;
    }

    public Task<IEnumerable<IDataContainer>> GetRowsAsDataContainer()
    {
        EnsureResult();

        if (ResultsAreEmpty())
        {
            return Task.FromResult<IEnumerable<IDataContainer>>([]);
        }

        string[] columnNames = new[] { "Key", "Value" };
        var containers = result!.Tables[0].Rows.OfType<DataRow>().Select(row =>
        {
            var data = new DataContainer
            {
                [ROW_IDENTIFIER_COLUMN] = GetRowIdentifier((string)row["Key"]),
            };
            foreach (string col in columnNames)
            {
                data[col] = row[col];
            }

            return data;
        });

        return Task.FromResult<IEnumerable<IDataContainer>>(containers);
    }

    public Task<string> GetRowAsText(Guid rowIdentifier)
    {
        EnsureResult();

        var row = FindRow(rowIdentifier) ?? throw new InvalidOperationException($"Failed to load row {rowIdentifier}");
        var textResult = new StringBuilder();
        string[] columnNames = new[] { "Key", "Value" };
        foreach (string col in columnNames)
        {
            textResult
                .Append($"<strong>{col}</strong>")
                .Append(": ")
                .Append(row[col])
                .Append(Environment.NewLine);
        }

        return Task.FromResult(textResult.ToString());
    }

    public bool Remove(Guid rowIdentifier)
    {
        EnsureResult();

        var row = FindRow(rowIdentifier);
        if (row is null)
        {
            return false;
        }

        // Keys come straight from the cache, so match them exactly and propagate the removal to other web farm servers
        CacheHelper.Remove((string)row["Key"], caseSensitive: true, logTask: true);
        return true;
    }

    public int RemoveByKeyContains(string cacheKeyContains)
    {
        // Collect the keys first so the cache isn't modified while it's being enumerated
        List<string> keys = [];
        var cacheEntries = cacheAccessor.GetEnumerator();
        while (cacheEntries.MoveNext())
        {
            string? key = cacheEntries.Entry.Key?.ToString();
            if (key is not null && key.Contains(cacheKeyContains, StringComparison.OrdinalIgnoreCase))
            {
                keys.Add(key);
            }
        }

        foreach (string key in keys)
        {
            CacheHelper.Remove(key, caseSensitive: true, logTask: true);
        }

        return keys.Count;
    }

    /// <summary>
    /// Gets a stable identifier for a cache key, so a row keeps its identifier even when other cache items are added or removed.
    /// </summary>
    private static Guid GetRowIdentifier(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes(key)).AsSpan(0, 16));

    /// <summary>
    /// Finds the row in <see cref="result"/> whose cache key matches the given identifier.
    /// </summary>
    private DataRow? FindRow(Guid rowIdentifier)
    {
        if (ResultsAreEmpty())
        {
            return null;
        }

        return result!.Tables[0].Rows.OfType<DataRow>().FirstOrDefault(row => GetRowIdentifier((string)row["Key"]) == rowIdentifier);
    }

    /// <summary>
    /// Ensures <see cref="result"/> is not null and if so, gets the cache items.
    /// </summary>
    private void EnsureResult()
    {
        try
        {
            List<CacheItem> cacheItems = [];
            var cacheEntries = cacheAccessor.GetEnumerator();
            while (cacheEntries.MoveNext()) // Use while loop to iterate over IDictionaryEnumerator
            {
                var item = cacheEntries.Entry;
                if (item.Key is not null) // Ensure item.Key is not null
                {
                    object? cacheItem = CacheHelper.GetItem(item.Key.ToString());
                    cacheItems.Add(new CacheItem
                    {
                        Key = item.Key.ToString()!, // Use null-forgiving operator to suppress CS8601
                        Value = cacheItem?.ToString() ?? string.Empty, // Handle potential null value
                        Expiration = null // Assuming Expiration is not available in IDictionaryEnumerator
                    });
                }
            }
            result = ConvertToDataSet(cacheItems);
            eventLogService.LogInformation(nameof(CacheManagerProvider), nameof(EnsureResult),
                $"Successfully retrieved the cache items.");
        }
        catch (Exception ex)
        {
            result = new DataSet();
            eventLogService.LogException(nameof(CacheManagerProvider), nameof(EnsureResult), ex);
        }
    }

    private DataSet ConvertToDataSet(List<CacheItem> cacheItems)
    {
        var dataSet = new DataSet();
        var dataTable = new DataTable("CacheItems");

        // Define columns
        dataTable.Columns.Add("Key", typeof(string));
        dataTable.Columns.Add("Value", typeof(string));

        // Add rows
        foreach (var item in cacheItems)
        {
            dataTable.Rows.Add(item.Key, item.Value);
        }

        dataSet.Tables.Add(dataTable);
        return dataSet;
    }

    /// <summary>
    /// Returns true if <see cref="result"/> is null or contains no tables.
    /// </summary>
    private bool ResultsAreEmpty() => result is null || result.Tables.Count == 0;
}
