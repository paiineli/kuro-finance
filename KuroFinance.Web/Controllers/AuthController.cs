using System.Security.Claims;
using KuroFinance.Data.Entities;
using KuroFinance.Data.Repositories.Interfaces;
using KuroFinance.Web.Models;
using KuroFinance.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace KuroFinance.Web.Controllers;

public class AuthController(IUserRepository userRepo, IEmailService emailService) : Controller
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

        if (!user.EmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "Confirme seu e-mail antes de entrar.");
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

        var confirmToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = vm.Name.Trim(),
            Email = vm.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password),
            EmailConfirmed = false,
            EmailConfirmationToken = confirmToken,
            EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        var confirmUrl = Url.Action("ConfirmEmail", "Auth", new { token = confirmToken }, Request.Scheme)!;
        await emailService.SendConfirmationEmailAsync(user.Email, user.Name, confirmUrl);

        TempData["Success"] = "Conta registrada! Verifique seu e-mail para ativar o acesso.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        var user = await userRepo.GetByConfirmationTokenAsync(token);

        if (user is null || user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
        {
            TempData["Error"] = "Link inválido ou expirado.";
            return RedirectToAction("Login");
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;
        await userRepo.UpdateAsync(user);
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
            user = new User { Id = Guid.NewGuid(), Name = name, Email = email, GoogleId = googleId, EmailConfirmed = true };
            await userRepo.AddAsync(user);
            await userRepo.SaveChangesAsync();
        }
        else if (user.GoogleId is null)
        {
            user.GoogleId = googleId;
            user.EmailConfirmed = true;
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
