using System.Security.Claims;
using KuroFinance.Data.Entities;
using KuroFinance.Data.Repositories.Interfaces;
using KuroFinance.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace KuroFinance.Web.Controllers;

public class AuthController(IUserRepository userRepo) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await userRepo.GetByEmailAsync(vm.Email);
        if (user is null || user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(vm.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
            return View(vm);
        }

        await SignInAsync(user);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var existing = await userRepo.GetByEmailAsync(vm.Email);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(vm.Email), "Este e-mail já está cadastrado.");
            return View(vm);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = vm.Name.Trim(),
            Email = vm.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password),
            CreatedAt = DateTime.UtcNow
        };

        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        await SignInAsync(user);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded) return RedirectToAction("Login");

        var googleId = result.Principal!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var email    = result.Principal!.FindFirstValue(ClaimTypes.Email)!;
        var name     = result.Principal!.FindFirstValue(ClaimTypes.Name) ?? email;

        var user = await userRepo.GetByGoogleIdAsync(googleId)
                ?? await userRepo.GetByEmailAsync(email);

        if (user is null)
        {
            user = new User { Id = Guid.NewGuid(), Name = name, Email = email, GoogleId = googleId };
            await userRepo.AddAsync(user);
            await userRepo.SaveChangesAsync();
        }
        else if (user.GoogleId is null)
        {
            user.GoogleId = googleId;
            await userRepo.UpdateAsync(user);
            await userRepo.SaveChangesAsync();
        }

        await HttpContext.SignOutAsync("External");
        await SignInAsync(user);
        return RedirectToAction("Index", "Dashboard");
    }

    private async Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Name),
            new(ClaimTypes.Email,          user.Email),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });
    }
}
