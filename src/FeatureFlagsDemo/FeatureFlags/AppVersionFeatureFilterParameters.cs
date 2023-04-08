using FeatureFlagsDemo.Data;

namespace FeatureFlagsDemo.FeatureFlags;

public class AppVersionFeatureFilterParameters
{
    public Dictionary<string, string> Versions { get; set; } = new();

    public FeatureStatus GetFeatureStatus(AppVersion appVersion)
    {
        if (!Versions.TryGetValue(appVersion.ToString(), out var status))
        {
            return FeatureStatus.Disabled;
        }

        if (!Enum.TryParse<FeatureStatus>(status, out var parsedStatus))
        {
            return FeatureStatus.Disabled;
        }

        return parsedStatus;
    }
}
