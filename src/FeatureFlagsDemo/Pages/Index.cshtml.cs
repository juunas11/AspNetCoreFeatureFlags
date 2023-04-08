using FeatureFlagsDemo.Data;
using FeatureFlagsDemo.Extensions;
using FeatureFlagsDemo.FeatureFlags;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;

namespace FeatureFlagsDemo.Pages;
public class IndexModel : PageModel
{
    private readonly InMemoryUserStore _userStore;
    private readonly IFeatureDefinitionProvider _featureDefinitionProvider;

    public IndexModel(InMemoryUserStore userStore, IFeatureDefinitionProvider featureDefinitionProvider)
    {
        _userStore = userStore;
        _featureDefinitionProvider = featureDefinitionProvider;
    }

    public Guid? UserId { get; set; }
    public AppVersion PreferredAppVersion { get; set; }
    public Dictionary<string, bool> OptInFeatures { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = HttpContext.GetUserId();
        if (userId.HasValue && !_userStore.UserExists(userId.Value))
        {
            // Old cookie, in-memory user store no longer has this user
            HttpContext.RemoveUserId();
            return;
        }

        UserId = userId;
        if (userId.HasValue)
        {
            PreferredAppVersion = _userStore.GetPreferredAppVersion(userId.Value);
            await foreach(var definition in _featureDefinitionProvider.GetAllFeatureDefinitionsAsync())
            {
                var filterConfig = definition.EnabledFor.FirstOrDefault(x => x.Name == "AppVersion");
                if (filterConfig == null)
                {
                    continue;
                }

                var parameters = filterConfig.Parameters.Get<AppVersionFeatureFilterParameters>();
                var featureStatus = parameters.GetFeatureStatus(PreferredAppVersion);
                var isOptInFeature = featureStatus == FeatureStatus.OptIn;
                if (isOptInFeature)
                {
                    var isUserOptedIn = _userStore.IsOptedInToFeature(userId.Value, definition.Name);
                    OptInFeatures.Add(definition.Name, isUserOptedIn);
                }
            }
        }
    }
}
