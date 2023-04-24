using FeatureFlagsDemo.Data;
using FeatureFlagsDemo.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlagsDemo.Controllers;

[Route("/users")]
public class UserController : Controller
{
    private readonly InMemoryUserStore _userStore;

    public UserController(InMemoryUserStore userStore)
    {
        _userStore = userStore;
    }

    [HttpPost("signup")]
    public IActionResult Signup()
    {
        var user = _userStore.CreateNewUser();
        HttpContext.SetUserId(user.Id);
        return RedirectToPage("/Index");
    }

    [HttpPost("regenerateid")]
    public IActionResult RegenerateId()
    {
        var userId = HttpContext.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToPage("/Index");
        }

        _userStore.RemoveUser(userId.Value);

        var user = _userStore.CreateNewUser();
        HttpContext.SetUserId(user.Id);
        return RedirectToPage("/Index");
    }
}
