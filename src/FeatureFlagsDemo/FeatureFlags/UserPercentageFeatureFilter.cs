using FeatureFlagsDemo.Extensions;
using Microsoft.FeatureManagement;

namespace FeatureFlagsDemo.FeatureFlags;

[FilterAlias("UserPercentage")]
public class UserPercentageFeatureFilter : IFeatureFilter
{
    private static readonly char[] GuidChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserPercentageFeatureFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context, CancellationToken cancellationToken)
    {
        var parameters = context.Parameters.Get<UserPercentageFeatureFilterParameters>() ?? new();
        var percentage = parameters.Percentage;
        if (percentage < 0 || percentage > 100)
        {
            // Invalid value
            return Task.FromResult(false);
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext.GetUserId();
        if (!userId.HasValue)
        {
            return Task.FromResult(false);
        }

        if (percentage == 0)
        {
            return Task.FromResult(false);
        }

        if (percentage == 100)
        {
            return Task.FromResult(true);
        }

        var userIdLastChar = userId.Value.ToString().Last();
        var enabledUpToCharIndex = (int)Math.Round(percentage / 100f * GuidChars.Length);
        var enabledChars = GuidChars.Take(enabledUpToCharIndex);
        var enabled = enabledChars.Any(c => c == userIdLastChar);

        return Task.FromResult(enabled);
    }
}
