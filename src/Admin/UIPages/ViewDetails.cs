using CMS.Helpers;
using CMS.Membership;

using System.Text.RegularExpressions;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.CacheManager.Services;

namespace XperienceCommunity.CacheManager.Admin.UIPages;

/// <summary>
///  UI page which displays the full details of the cache item in a dialog window.
/// </summary>
[UINavigation(false)]
[UIBreadcrumbs(false)]
[UIPageLocation(PageLocationEnum.Dialog)]
[UIEvaluatePermission(SystemPermissions.VIEW)]
public class ViewDetails(IFormDataBinder formDataBinder, ICacheManagerProvider cacheManagerProvider) : EditPageBase(formDataBinder)
{
    private const string EOL_REPLACEMENT = "#EOL#";
    private readonly Regex newLineRegex = RegexHelper.GetRegex(@"(<br[ ]?/>)|([\r]?\n)");


    /// <summary>
    /// Identifier of the result record to view.
    /// </summary>
    [PageParameter(typeof(GuidPageModelBinder))]
    public Guid RecordId { get; set; }


    public override async Task ConfigurePage()
    {
        PageConfiguration.Disabled = true;
        PageConfiguration.SubmitConfiguration.Visible = false;

        await base.ConfigurePage();
    }


    public override async Task<EditTemplateClientProperties> ConfigureTemplateProperties(EditTemplateClientProperties properties)
    {
        string text = await cacheManagerProvider.GetRowAsText(RecordId);
        text = newLineRegex.Replace(text, EOL_REPLACEMENT);
        properties.Items = [
            new TextWithLabelClientProperties()
            {
                Disabled = true,
                ValueAsHtml = true,
                ComponentName = "@kentico/xperience-admin-base/TextWithLabel",
                Value = text.Replace(EOL_REPLACEMENT, "<br />"),
            }
        ];

        return properties;
    }


    protected override Task<ICollection<IFormItem>> GetFormItems() =>
        Task.FromResult(Array.Empty<IFormItem>() as ICollection<IFormItem>);


    protected override Task<IEnumerable<IFormItemClientProperties>> GetFormItemsClientProperties() =>
        Task.FromResult(Enumerable.Empty<IFormItemClientProperties>());


    protected override Task<ICommandResponse> SubmitInternal(
        FormSubmissionCommandArguments args,
        ICollection<IFormItem> items,
        IFormFieldValueProvider formFieldValueProvider) => throw new NotImplementedException();
}
