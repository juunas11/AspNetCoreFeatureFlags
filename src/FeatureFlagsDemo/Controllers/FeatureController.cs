using FeatureFlagsDemo.Data;
using FeatureFlagsDemo.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace FeatureFlagsDemo.Controllers;

[Route("/feature")]
public class FeatureController : Controller
{
    private readonly InMemoryUserStore _userStore;
    private readonly IFeatureManager _featureManager;

    public FeatureController(InMemoryUserStore userStore, IFeatureManager featureManager)
    {
        _userStore = userStore;
        _featureManager = featureManager;
    }

    [HttpPost("preferredappversion")]
    public IActionResult SetPreferredAppVersion([FromForm] AppVersion appVersion)
    {
        var userId = HttpContext.GetUserId();
        _userStore.SetPreferredAppVersion(userId.Value, appVersion);
        return RedirectToPage("/Index");
    }

    [HttpPost("optinfeatures")]
    public async Task<IActionResult> SetOptInFeatures([FromForm] Dictionary<string, string> features)
    {
        var userId = HttpContext.GetUserId();

        await foreach (var feature in _featureManager.GetFeatureFlagNamesAsync())
        {
            if (features.ContainsKey(feature))
            {
                _userStore.OptInToFeature(userId.Value, feature);
            }
            else
            {
                _userStore.OptOutOfFeature(userId.Value, feature);
            }
        }

        return RedirectToPage("/Index");
    }
}
