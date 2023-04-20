namespace FeatureFlagsDemo.FeatureFlags;

public static class Features
{
    // In development, large-ish feature
    public const string NewsSummary = nameof(NewsSummary);

    // Enabled in beta for all users, stable users can opt-in
    public const string DarkTheme = nameof(DarkTheme);

    // Enabled for portion of users
    public const string UserGreeting = nameof(UserGreeting);
}
