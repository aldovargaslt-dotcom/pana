using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Identity;

namespace Pana.Api.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    [HttpGet("login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromForm] string email,
        [FromForm] string password,
        [FromServices] IAuthService authService,
        CancellationToken ct)
    {
        var result = await authService.LoginAsync(new LoginRequest(email, password), ct);
        if (result is null)
        {
            ModelState.AddModelError("", "Credenciales inválidas.");
            return View();
        }

        // Store JWT in a cookie
        Response.Cookies.Append("pana_token", result.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("pana_token");
        return RedirectToAction("Login");
    }
}
