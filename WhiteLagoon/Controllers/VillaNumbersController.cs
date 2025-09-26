using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.ViewModels;

namespace WhiteLagoon.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class VillaNumbersController(IVillaNumberService villaNumberService, IVillaService villaService) : Controller
{
	public async Task<IActionResult> Index()
	{
		var villaNumbers = await villaNumberService.GetAllVillaNumbersAsync();

		return View(villaNumbers);
	}

	public async Task<IActionResult> Create()
	{
		var villasSelectItems = (await villaService.GetAllVillasAsync())
			.Select(v => new SelectListItem
			{
				Value = v.Id.ToString(),
				Text = v.Name.ToString()
			})
			.ToList();

		VillaNumberViewModel viewModel = new()
		{
			VillaNumber = new(),
			VillaList = villasSelectItems
		};

		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> Create(VillaNumber villaNumber)
	{
		var villaNumberAlreadyExists = await villaNumberService.ExistsAsync(villaNumber.Villa_Number);

		if (villaNumberAlreadyExists)
		{
			var msg = "Villa number already exists for the selected villa.";
			TempData["error"] = msg;
			ModelState.AddModelError(nameof(villaNumber.Villa_Number), msg);
		}

		if (!ModelState.IsValid || villaNumberAlreadyExists)
		{
			var villasSelectItems = (await villaService.GetAllVillasAsync())
				.Select(v => new SelectListItem
				{
					Value = v.Id.ToString(),
					Text = v.Name.ToString()
				})
				.ToList();

			return View(new VillaNumberViewModel { VillaNumber = villaNumber, VillaList = villasSelectItems });
		}

		await villaNumberService.CreateVillaNumberAsync(villaNumber);

		TempData["success"] = "Villa number created successfully.";

		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Update(int villaNumberId)
	{
		var villaList = await villaService.GetAllVillasAsync();
		var villaNumber = await villaNumberService.GetVillaNumberByIdAsync(villaNumberId);

		if (villaNumber is null)
		{
			TempData["error"] = "Villa number not found";
			return RedirectToAction("Error", "Home");
		}

		var viewModel = new VillaNumberViewModel
		{
			VillaList = [.. villaList.Select(x => new SelectListItem
			{
				Text = x.Name,
				Value = x.Id.ToString()
			})],
			VillaNumber = villaNumber
		};

		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> Update(VillaNumber villaNumber)
	{
		if (!ModelState.IsValid || villaNumber.Villa_Number == 0)
			return RedirectToAction(nameof(Index));

		if (await villaNumberService.ExistsAsync(villaNumber.Villa_Number))
		{
			TempData["error"] = "Villa Number with same number already exists";
			return RedirectToAction(nameof(Index));
		}

		await villaNumberService.UpdateVillaNumberAsync(villaNumber);

		TempData["success"] = "Villa Number updated successfully.";

		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Delete(int villaNumberId)
	{
		var villaNumber = await villaNumberService.GetVillaNumberByIdAsync(villaNumberId);

		if (villaNumber is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		var villaList = await villaService.GetAllVillasAsync();

		var viewModel = new VillaNumberViewModel
		{
			VillaList = [.. villaList.Select(x => new SelectListItem
			{
				Text = x.Name,
				Value = x.Id.ToString()
			})],
			VillaNumber = villaNumber
		};

		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> Delete(VillaNumberViewModel villaNumberViewModel)
	{
		if (villaNumberViewModel.VillaNumber is null)
		{
			TempData["error"] = "Villa not found.";
			return RedirectToAction(nameof(Index));
		}

		var villaNumberToDelete = await villaNumberService.GetVillaNumberByIdAsync(villaNumberViewModel.VillaNumber.Villa_Number);

		if (villaNumberToDelete is not null)
		{
			await villaNumberService.DeleteVillaNumberAsync(villaNumberToDelete);

			TempData["success"] = "Villa deleted successfully.";
		}

		TempData["error"] = "Villa not found.";

		return RedirectToAction(nameof(Index));
	}
}