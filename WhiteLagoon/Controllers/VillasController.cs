using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class VillasController(
    IVillaService villaService,
    IWebHostEnvironment webHostEnvironment) : Controller
{
    public async Task<IActionResult> Index()
    {
        var villas = await villaService.GetAllVillasAsync();

        return View(villas);
    }

    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Villa villa)
    {
        if(!ModelState.IsValid)
			return View();

        if(villa.Name == villa.Description)
        {
            ModelState.AddModelError(nameof(Villa.Name), "The Name and Description cannot be the same.");
			return View(villa);
		}

        await villaService.CreateVillaAsync(villa, webHostEnvironment.WebRootPath);

        TempData["success"] = "Villa created successfully.";    

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int villaId)
    {
        Villa? villa = await villaService.GetVillaByIdAsync(villaId);

        if(villa is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		return View(villa);
    }
    
    [HttpPost]
	public async Task<IActionResult> Update(Villa villa)
    {
		if (!ModelState.IsValid || villa.Id == 0)
			return View();

		if (villa.Name == villa.Description)
        {
            ModelState.AddModelError(nameof(Villa.Name), "The Name and Description cannot be the same.");
			return View(villa);
		}

		await villaService.UpdateVillaAsync(villa, webHostEnvironment.WebRootPath);

        TempData["success"] = "Villa updated successfully.";

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int villaId)
    {
        Villa? villa = await villaService.GetVillaByIdAsync(villaId);
        
        if(villa is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		return View(villa);
    }

    [HttpPost]
	public async Task<IActionResult> Delete(Villa villa)
    {
        var villaToDelete = await villaService.GetVillaByIdAsync(villa.Id);
        
		if (villaToDelete is not null)
        {
		    await villaService.DeleteVillaAsync(villa, webHostEnvironment.WebRootPath);

            TempData["success"] = "Villa deleted successfully.";

			return RedirectToAction(nameof(Index));
		}

        TempData["error"] = "Villa not found.";

		return View();
    }
}