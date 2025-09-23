using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.ViewModels;

namespace WhiteLagoon.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class AmenitiesController(IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Index()
    {
        var amenities = await unitOfWork.Amenities.GetAllAsync(includeProperties: nameof(Amenity.Villa));

        return View(amenities);
    }

    public async Task<IActionResult> Create()
    {
        return View(await GetAmenityViewModelAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Amenity amenity)
    {
        if(!ModelState.IsValid)
			return View();

	    await unitOfWork.Amenities.AddAsync(amenity);
        await unitOfWork.SaveAsync();

        TempData["success"] = "Amenity created successfully.";    

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int amenityId)
    {
        Amenity? amenity = await unitOfWork.Amenities.GetAsync(v => v.Id == amenityId);

        if(amenity is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		return View(await GetAmenityViewModelAsync(amenity));
    }
    
    [HttpPost]
	public async Task<IActionResult> Update(Amenity amenity)
    {
		if (!ModelState.IsValid || amenity.Id == 0)
			return View();
	
		unitOfWork.Amenities.Update(amenity);
        await unitOfWork.SaveAsync();

        TempData["success"] = "Amenity updated successfully.";

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int amenityId)
    {
        Amenity? amenity = await unitOfWork.Amenities.GetAsync(v => v.Id == amenityId);
        
        if(amenity is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		return View(await GetAmenityViewModelAsync(amenity));
    }

    [HttpPost]
	public async Task<IActionResult> Delete(Amenity amenity)
    {
        var amenityToDelete = await unitOfWork.Amenities.GetAsync(v => v.Id == amenity.Id);
        
		if (amenityToDelete is not null)
        {
		    unitOfWork.Amenities.Remove(amenityToDelete);
			await unitOfWork.SaveAsync();

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
            VillaList = [.. (await unitOfWork.Villas.GetAllAsync())
                .Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                })]
		};
        return amenityViewModel;
	}
}