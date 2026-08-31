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
    IUIPermissionEvaluator permissionEvaluator) : DataContainerListingPage
{
    protected override object GetIdentifier(IDataContainer dataContainer) =>
        ValidationHelper.GetInteger(dataContainer[CacheManagerProvider.ROW_IDENTIFIER_COLUMN], -1);


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
            PageConfiguration.AddEditRowAction<ViewDetails>();
        }

        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.UPDATE)]
    public Task<ICommandResponse> ClearCache() => Clear();


    private void ConfigureColumns()
    {
        string[] columnNames = new[] { "Key", "Value" };

        // Get largest header length to set all column min width- avoids text jumbling
        int columnMinWidth = Math.Ceiling(columnNames.Max(col => col.Length) * 0.7).ToInteger();
        foreach (string col in columnNames)
        {
            PageConfiguration.ColumnConfigurations.AddColumn(col, col, minWidth: columnMinWidth);
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
