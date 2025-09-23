using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.ViewModels;

namespace WhiteLagoon.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class VillaNumbersController(IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Index()
    {
        var villaNumbers = await unitOfWork.VillaNumbers.GetAllAsync(includeProperties: nameof(VillaNumber.Villa));

        return View(villaNumbers);
    }

    public async Task<IActionResult> Create()
    {
        var villasSelectItems = (await unitOfWork.Villas.GetAllAsync())
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
        var villaNumberAlreadyExists = await unitOfWork.VillaNumbers
            .AnyAsync(vn => vn.Villa_Number == villaNumber.Villa_Number && vn.VillaId == villaNumber.VillaId);

        if (villaNumberAlreadyExists)
        {
            var msg = "Villa number already exists for the selected villa.";
            TempData["error"] = msg;
            ModelState.AddModelError(nameof(villaNumber.Villa_Number), msg);
        }

        if (!ModelState.IsValid || villaNumberAlreadyExists)
        {
            var villasSelectItems = (await unitOfWork.Villas.GetAllAsync())
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.Name.ToString()
                })
                .ToList();

            return View(new VillaNumberViewModel { VillaNumber = villaNumber, VillaList = villasSelectItems });
        }

	    await unitOfWork.VillaNumbers.AddAsync(villaNumber);
        await unitOfWork.SaveAsync();

        TempData["success"] = "Villa number created successfully.";    

		return RedirectToAction(nameof(Index));
    }

	public async Task<IActionResult> Update(int villaNumberId)
	{
		var villaList = await unitOfWork.Villas.GetAllAsync();
		var villaNumber = await unitOfWork.VillaNumbers.GetAsync(x => x.Villa_Number == villaNumberId);

        if(villaNumber is null)
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

        if(await unitOfWork.VillaNumbers.AnyAsync(v => v.Villa_Number == villaNumber.Villa_Number))
        {
            TempData["error"] = "Villa Number with same number already exists";
		    return RedirectToAction(nameof(Index));
        }

        unitOfWork.VillaNumbers.Update(villaNumber);
        await unitOfWork.SaveAsync();

        TempData["success"] = "Villa Number updated successfully.";

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int villaNumberId)
    {
        var villaNumber = await unitOfWork.VillaNumbers.GetAsync(v => v.Villa_Number == villaNumberId);

        if(villaNumber is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

        var villaList = await unitOfWork.Villas.GetAllAsync();

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
        if(villaNumberViewModel.VillaNumber is null)
        {
            TempData["error"] = "Villa not found.";
            return RedirectToAction(nameof(Index));
        }

        var villaNumberToDelete = await unitOfWork.VillaNumbers.GetAsync(v => v.Villa_Number == villaNumberViewModel.VillaNumber.Villa_Number);

		if (villaNumberToDelete is not null)
        {
			unitOfWork.VillaNumbers.Remove(villaNumberToDelete);
			await unitOfWork.SaveAsync();

            TempData["success"] = "Villa deleted successfully.";
		}

        TempData["error"] = "Villa not found.";

	    return RedirectToAction(nameof(Index));
    }
}