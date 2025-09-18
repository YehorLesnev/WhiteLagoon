using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.ViewModels.Auth;

namespace WhiteLagoon.Controllers;

public class AccountController(
	IUnitOfWork unitOfWork,
	UserManager<ApplicationUser> userManager,
	SignInManager<ApplicationUser> signInManager,
	RoleManager<IdentityRole> roleManager) : Controller
{
	public IActionResult Login(string? returnUrl = null)
	{
		returnUrl ??= Url.Content("~/");

		var loginViewModel = new LoginViewModel
		{
			RedirectUrl = returnUrl
		};

		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Login(LoginViewModel loginViewModel)
	{
		if(!ModelState.IsValid)
		{
			return View(loginViewModel);
		}
		
		var result = await signInManager
				.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, false);

		if (result.Succeeded)
		{
			if (!string.IsNullOrEmpty(loginViewModel.RedirectUrl))
			{
				return LocalRedirect(loginViewModel.RedirectUrl);
			}

			return RedirectToAction(nameof(HomeController.Index), "Home");
		}

		ModelState.AddModelError(string.Empty, "Invalid login attempt.");

		return View(loginViewModel);
	}

	public async Task<IActionResult> Logout()
	{
		await signInManager.SignOutAsync();
		return RedirectToAction(nameof(HomeController.Index), "Home");
	}

	public async Task<IActionResult> Register()
	{
		if (!await roleManager.RoleExistsAsync(RolesConstants.Admin))
			await roleManager.CreateAsync(new IdentityRole(RolesConstants.Admin));

		if (!await roleManager.RoleExistsAsync(RolesConstants.Customer))
			await roleManager.CreateAsync(new IdentityRole(RolesConstants.Customer));

		var viewModel = await GetRegisterViewModelAsync();

		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(await GetRegisterViewModelAsync(registerViewModel));
		}

		ApplicationUser user = new()
		{
			Name = registerViewModel.Name,
			Email = registerViewModel.Email,
			PhoneNumber = registerViewModel.PhoneNumber,
			NormalizedEmail = registerViewModel.Email.ToUpper(),
			EmailConfirmed = true,
			UserName = registerViewModel.Email,
			CreatedAt = DateTime.Now
		};

		var result = await userManager.CreateAsync(user, registerViewModel.Password);

		if (!result.Succeeded)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View(await GetRegisterViewModelAsync(registerViewModel));
		}

		if (!string.IsNullOrEmpty(registerViewModel.Role))
		{
			await userManager.AddToRoleAsync(user, registerViewModel.Role);
		}
		else
		{
			await userManager.AddToRoleAsync(user, RolesConstants.Customer);
		}

		await signInManager.SignInAsync(user, false);

		if (!string.IsNullOrEmpty(registerViewModel.RedirectUrl))
		{
			return LocalRedirect(registerViewModel.RedirectUrl);
		}

		return RedirectToAction(nameof(HomeController.Index), "Home");
	}

	private async Task<RegisterViewModel> GetRegisterViewModelAsync(RegisterViewModel? viewModel = null)
	{
		var roles = (await roleManager.Roles.ToListAsync())
			.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
			{
				Text = r.Name,
				Value = r.Name
			});

		if (viewModel is not null)
		{
			viewModel.Roles = roles;
			return viewModel;
		}

		return new RegisterViewModel
		{
			Roles = roles
		};
	}
}