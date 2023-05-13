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

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var userId = httpContext.GetUserId();
        if (!userId.HasValue)
        {
            return Task.FromResult(false);
        }

        var userStore = httpContext.RequestServices.GetRequiredService<InMemoryUserStore>();
        if (!userStore.UserExists(userId.Value))
        {
            return Task.FromResult(false);
        }

        var preferredVersion = userStore.GetPreferredAppVersion(userId.Value);

        var parameters = context.Parameters.Get<AppVersionFeatureFilterParameters>() ?? new();
        var status = parameters.GetFeatureStatus(preferredVersion);

        var isEnabled = status switch
        {
            FeatureStatus.Enabled => true,
            FeatureStatus.OptIn => userStore.IsOptedInToFeature(userId.Value, context.FeatureFlagName),
            FeatureStatus.Disabled => false,
            _ => throw new NotImplementedException(),
        };

        return Task.FromResult(isEnabled);
    }
}
