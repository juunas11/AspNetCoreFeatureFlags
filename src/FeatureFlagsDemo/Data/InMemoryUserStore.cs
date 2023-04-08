namespace FeatureFlagsDemo.Data;

public class InMemoryUserStore
{
    private readonly Dictionary<Guid, UserModel> _users = new();

    public UserModel CreateNewUser()
    {
        var user = new UserModel();
        _users.Add(user.Id, user);
        return user;
    }

    public bool UserExists(Guid userId)
    {
        return _users.ContainsKey(userId);
    }

    public AppVersion GetPreferredAppVersion(Guid userId)
    {
        return _users[userId].PreferredAppVersion;
    }

    public void SetPreferredAppVersion(Guid userId, AppVersion appVersion)
    {
        _users[userId].PreferredAppVersion = appVersion;
    }

    public bool IsOptedInToFeature(Guid userId, string feature)
    {
        return _users[userId].OptedInFeatures.Contains(feature);
    }

    public void OptInToFeature(Guid userId, string feature)
    {
        var optedInFeatures = _users[userId].OptedInFeatures;
        if (optedInFeatures.Contains(feature))
        {
            return;
        }

        optedInFeatures.Add(feature);
    }

    public void OptOutOfFeature(Guid userId, string feature)
    {
        _users[userId].OptedInFeatures.Remove(feature);
    }
}
