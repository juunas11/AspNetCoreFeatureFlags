using FeatureFlagsDemo.Data;
using FeatureFlagsDemo.Extensions;
using Microsoft.FeatureManagement;

namespace FeatureFlagsDemo.FeatureFlags;

[FilterAlias("OptIn")]
public class OptInFeatureFilter : IFeatureFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OptInFeatureFilter(IHttpContextAccessor httpContextAccessor)
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

        var userStore = httpContext.RequestServices.GetRequiredService<InMemoryUserStore>();
        var isOptedIn = userStore.IsOptedInToFeature(userId.Value, context.FeatureName);

        return Task.FromResult(isOptedIn);
    }
}
