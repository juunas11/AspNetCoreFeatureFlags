using FeatureFlagsDemo.Data;
using FeatureFlagsDemo.Extensions;
using Microsoft.FeatureManagement;

namespace FeatureFlagsDemo.FeatureFlags;

[FilterAlias("AppVersion")]
public class AppVersionFeatureFilter : IFeatureFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppVersionFeatureFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var userId = httpContext.GetUserId();
        if (!userId.HasValue)
        {
            return Task.FromResult(false);
        }

        var parameters = context.Parameters.Get<AppVersionFeatureFilterParameters>() ?? new();

        var userStore = httpContext.RequestServices.GetRequiredService<InMemoryUserStore>();
        var preferredVersion = userStore.GetPreferredAppVersion(userId.Value);

        var isEnabled = parameters.IsAppVersionEnabled(preferredVersion);

        return Task.FromResult(isEnabled);
    }
}

public class AppVersionFeatureFilterParameters
{
    public List<string> Versions { get; set; } = new();

    public bool IsAppVersionEnabled(AppVersion appVersion)
    {
        return Versions.Contains(appVersion.ToString());
    }
}