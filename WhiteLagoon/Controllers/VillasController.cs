using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Controllers;

public class VillasController(IVillaRepository villaRepository) : Controller
{
    public async Task<IActionResult> Index()
    {
        var villas = await villaRepository.GetAllAsync();

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

	    await villaRepository.AddAsync(villa);
        await villaRepository.SaveAsync();

        TempData["success"] = "Villa created successfully.";    

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int villaId)
    {
        Villa? villa = await villaRepository.GetAsync(v => v.Id == villaId);

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

        villaRepository.Update(villa);
        await villaRepository.SaveAsync();

        TempData["success"] = "Villa updated successfully.";

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int villaId)
    {
        Villa? villa = await villaRepository.GetAsync(v => v.Id == villaId);

        if(villa is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		return View(villa);
    }

    [HttpPost]
	public async Task<IActionResult> Delete(Villa villa)
    {
        var villaToDelete = await villaRepository.GetAsync(v => v.Id == villa.Id);

		if (villaToDelete is not null)
        {
		    villaRepository.Remove(villa);
			await villaRepository.SaveAsync();

            TempData["success"] = "Villa deleted successfully.";

			return RedirectToAction(nameof(Index));
		}

        TempData["error"] = "Villa not found.";

		return View();
    }
}