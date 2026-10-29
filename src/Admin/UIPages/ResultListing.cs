using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;

using XperienceCommunity.CacheManager.Services;

namespace XperienceCommunity.CacheManager.Admin.UIPages;

/// <summary>
/// Listing UI page which displays the results of the cache query in a table format.
/// </summary>
[UINavigation(false)]
[UIEvaluatePermission(SystemPermissions.VIEW)]
public class ResultListing(
    ICacheManagerProvider cacheManagerProvider,
    IEventLogService eventLogService,
    IUIPermissionEvaluator permissionEvaluator,
    CacheManagerOptions options) : DataContainerListingPage
{
    protected override object GetIdentifier(IDataContainer dataContainer) =>
        ValidationHelper.GetGuid(dataContainer[CacheManagerProvider.ROW_IDENTIFIER_COLUMN], Guid.Empty);


    protected override Task<IEnumerable<IDataContainer>> LoadDataContainers(CancellationToken cancellationToken) =>
        cacheManagerProvider.GetRowsAsDataContainer();

    public override async Task ConfigurePage()
    {
        int recordCount = cacheManagerProvider.Count();
        if (recordCount == 0)
        {
            PageConfiguration.Callouts = [
                new CalloutConfiguration
                {
                    Headline = "No Cache Items Found",
                    Content = "There are no cache items to display.",
                    Placement = CalloutPlacement.OnPaper,
                    Type = CalloutType.FriendlyWarning
                }];
        }
        else
        {
            ConfigureColumns();
            PageConfiguration.Caption = $"Cache Items Found ({recordCount})";
            PageConfiguration.HeaderActions.AddCommand("Clear Cache", nameof(ClearCache));
            for (int i = 0; i < options.ClearCacheButtons.Count; i++)
            {
                PageConfiguration.HeaderActions.AddCommand(options.ClearCacheButtons[i].ButtonText, $"{CLEAR_CACHE_BY_KEY_COMMAND_PREFIX}{i}");
            }
            PageConfiguration.AddEditRowAction<ViewDetails>();
            PageConfiguration.TableActions.AddCommandWithConfirmation(
                label: "Clear cache key",
                command: nameof(ClearCacheKey),
                icon: Icons.Broom,
                confirmation: "Clear cache key",
                confirmationDetail: "Are you sure you want to remove this item from the cache?",
                confirmationButton: "Clear",
                title: "Clear cache key",
                destructive: true);
        }

        await base.ConfigurePage();
    }

    [PageCommand]
    public Task<ICommandResponse> ClearCache() => Clear();

    // Header buttons can't pass data to their command, so each custom clear cache button gets its own command slot,
    // up to CacheManagerOptions.MAX_CLEAR_CACHE_BUTTONS
    private const string CLEAR_CACHE_BY_KEY_COMMAND_PREFIX = "ClearCacheByKey";

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey0() => ClearByKeyContains(0);

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey1() => ClearByKeyContains(1);

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey2() => ClearByKeyContains(2);

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey3() => ClearByKeyContains(3);

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey4() => ClearByKeyContains(4);

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey5() => ClearByKeyContains(5);

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey6() => ClearByKeyContains(6);

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey7() => ClearByKeyContains(7);

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey8() => ClearByKeyContains(8);

    [PageCommand]
    public Task<ICommandResponse> ClearCacheByKey9() => ClearByKeyContains(9);

    [PageCommand]
    public Task<ICommandResponse<RowActionResult>> ClearCacheKey(Guid id)
    {
        bool hasRemovedKey = false;
        try
        {
            hasRemovedKey = cacheManagerProvider.Remove(id);
        }
        catch (Exception ex)
        {
            eventLogService.LogException(nameof(ResultListing), nameof(ClearCacheKey), ex);
            return Task.FromResult(ResponseFrom(new RowActionResult(reload: false)).AddErrorMessage("Failed to clear the cache key."));
        }

        var response = ResponseFrom(new RowActionResult(reload: true, refetchAll: true));
        return Task.FromResult(hasRemovedKey
            ? response.AddSuccessMessage("The cache key was successfully cleared.")
            : response.AddInfoMessage("The cache key no longer exists. It may have already expired or been removed."));
    }


    private void ConfigureColumns()
    {
        string[] columnNames = new[] { "Key", "Value" };

        // Get largest header length to set all column min width- avoids text jumbling
        int columnMinWidth = Math.Ceiling(columnNames.Max(col => col.Length) * 0.7).ToInteger();
        foreach (string col in columnNames)
        {
            // Only the Key column is searchable; the listing's search box filters by Key using a case-insensitive contains
            PageConfiguration.ColumnConfigurations.AddColumn(col, col, minWidth: columnMinWidth, searchable: col == "Key");
        }
    }

    private Task<ICommandResponse> ClearByKeyContains(int buttonIndex)
    {
        if (buttonIndex >= options.ClearCacheButtons.Count)
        {
            return Task.FromResult<ICommandResponse>(Response().AddErrorMessage("This clear cache button is not configured."));
        }

        string cacheKeyContains = options.ClearCacheButtons[buttonIndex].CacheKeyContains;
        try
        {
            int removedCount = cacheManagerProvider.RemoveByKeyContains(cacheKeyContains);
            return Task.FromResult<ICommandResponse>(Response().AddSuccessMessage(
                $"Cleared {removedCount} cache item(s) whose key contains '{cacheKeyContains}'. Please refresh the page to view the updated results."));
        }
        catch (Exception ex)
        {
            eventLogService.LogException(nameof(ResultListing), nameof(ClearByKeyContains), ex);
            return Task.FromResult<ICommandResponse>(Response().AddErrorMessage($"Failed to clear the cache items whose key contains '{cacheKeyContains}'."));
        }
    }

    private async Task<ICommandResponse> Clear()
    {
        bool hasClearedCache = false;
        try
        {
            cacheManagerProvider.Clear();
            hasClearedCache = true;
        }
        catch (Exception ex)
        {
            eventLogService.LogException(nameof(ResultListing), nameof(Clear), ex);
        }

        if (hasClearedCache)
        {
            return Response().AddSuccessMessage($"The site cache was successfully cleared. Please refresh the page to view the updated results.");
        }
        else
        {
            return Response().AddErrorMessage("Failed to clear the site cache.");
        }
    }
}
