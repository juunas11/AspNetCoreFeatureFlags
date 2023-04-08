namespace FeatureFlagsDemo.Data;

public class UserModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AppVersion PreferredAppVersion { get; set; } = AppVersion.Stable;
    public List<string> OptedInFeatures { get; set; } = new();
}
