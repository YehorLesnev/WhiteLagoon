using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.ViewModels;

namespace WhiteLagoon.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class AmenitiesController(IAmenityService amenityService, IVillaService villaService) : Controller
{
	public async Task<IActionResult> Index()
	{
		var amenities = await amenityService.GetAllAmenitiesAsync();

		return View(amenities);
	}

	public async Task<IActionResult> Create()
	{
		return View(await GetAmenityViewModelAsync());
	}

	[HttpPost]
	public async Task<IActionResult> Create(Amenity amenity)
	{
		if (!ModelState.IsValid)
			return View();

		await amenityService.CreateAmenityAsync(amenity);

		TempData["success"] = "Amenity created successfully.";

		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Update(int amenityId)
	{
		Amenity? amenity = await amenityService.GetAmenityByIdAsync(amenityId);

		if (amenity is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		return View(await GetAmenityViewModelAsync(amenity));
	}

	[HttpPost]
	public async Task<IActionResult> Update(Amenity amenity)
	{
		if (!ModelState.IsValid || amenity.Id == 0)
			return View();

		await amenityService.UpdateAmenityAsync(amenity);

		TempData["success"] = "Amenity updated successfully.";

		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Delete(int amenityId)
	{
		Amenity? amenity = await amenityService.GetAmenityByIdAsync(amenityId);

		if (amenity is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		return View(await GetAmenityViewModelAsync(amenity));
	}

	[HttpPost]
	public async Task<IActionResult> Delete(Amenity amenity)
	{
		var amenityToDelete = await amenityService.GetAmenityByIdAsync(amenity.Id);

		if (amenityToDelete is not null)
		{
			await amenityService.DeleteAmenityAsync(amenityToDelete);

			TempData["success"] = "Amenity deleted successfully.";

			return RedirectToAction(nameof(Index));
		}

		TempData["error"] = "Amenity not found.";

		return View();
	}

	private async Task<AmenityViewModel> GetAmenityViewModelAsync(Amenity? amenity = null)
	{
		var amenityViewModel = new AmenityViewModel
		{
			Amenity = amenity ?? new(),
			VillaList = [.. (await villaService.GetAllVillasAsync())
				.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
				{
					Text = v.Name,
					Value = v.Id.ToString()
				})]
		};
		return amenityViewModel;
	}
}