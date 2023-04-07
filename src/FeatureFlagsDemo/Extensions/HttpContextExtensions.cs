namespace FeatureFlagsDemo.Extensions;

public static class HttpContextExtensions
{
    public const string UserIdCookieName = "DemoUserId";

    public static Guid? GetUserId(this HttpContext context)
    {
        // Note: this is only for demo purposes. Always use proper authentication schemes in your applications.

        if (!context.Request.Cookies.TryGetValue(UserIdCookieName, out var userId))
        {
            return null;
        }

        return Guid.Parse(userId);
    }

    public static void SetUserId(this HttpContext context, Guid userId)
    {
        context.Response.Cookies.Append(UserIdCookieName, userId.ToString(), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = true,
        });
    }

    public static void RemoveUserId(this HttpContext context)
    {
        context.Response.Cookies.Delete(UserIdCookieName);
    }
}
