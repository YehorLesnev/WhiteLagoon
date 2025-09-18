using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
		returnUrl??=Url.Content("~/");

		var loginViewModel = new LoginViewModel
		{
			RedirectUrl = returnUrl
		};

		return View();
	}

	public async Task<IActionResult> Register()
	{
		if(!await roleManager.RoleExistsAsync(RolesConstants.Admin))
			await roleManager.CreateAsync(new IdentityRole(RolesConstants.Admin));

		if(!await roleManager.RoleExistsAsync(RolesConstants.Customer))
			await roleManager.CreateAsync(new IdentityRole(RolesConstants.Customer));

		var roles = await roleManager.Roles.ToListAsync();
		var viewModel = new RegisterViewModel
		{
			Roles = roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
			{
				Text = r.Name,
				Value = r.Name
			})
		};

		return View(viewModel);
	}
}