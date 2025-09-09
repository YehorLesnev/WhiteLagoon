using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.ViewModels;

namespace WhiteLagoon.Controllers;

public class VillaNumbersController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var villaNumbers = await dbContext.VillaNumbers.Include(vn => vn.Villa).ToListAsync();

        return View(villaNumbers);
    }

    public async Task<IActionResult> Create()
    {
        var villasSelectItems = await dbContext.Villas
			.Select(v => new SelectListItem
			{
				Value = v.Id.ToString(),
				Text = v.Name.ToString()
			})
			.ToListAsync();

		VillaNumberViewModel viewModel = new()
		{
            VillaList = villasSelectItems
        };

		return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(VillaNumber villaNumber)
    {
        var villaNumberAlreadyExists = await dbContext.VillaNumbers
            .AnyAsync(vn => vn.Villa_Number == villaNumber.Villa_Number && vn.VillaId == villaNumber.VillaId);

        if (villaNumberAlreadyExists)
        {
            var msg = "Villa number already exists for the selected villa.";
            TempData["error"] = msg;
            ModelState.AddModelError(nameof(villaNumber.Villa_Number), msg);
        }

        if (!ModelState.IsValid || villaNumberAlreadyExists)
        {
            var villasSelectItems = await dbContext.Villas
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.Name.ToString()
                })
                .ToListAsync();

            return View(new VillaNumberViewModel { VillaNumber = villaNumber, VillaList = villasSelectItems });
        }

	    await dbContext.VillaNumbers.AddAsync(villaNumber);
        await dbContext.SaveChangesAsync();

        TempData["success"] = "Villa number created successfully.";    

		return RedirectToAction(nameof(Index));
    }

	public async Task<IActionResult> Update(int villaNumberId)
	{
		var villaList = await dbContext.Villas.ToListAsync();
		var villaNumber = await dbContext.VillaNumbers.FirstOrDefaultAsync(x => x.Villa_Number == villaNumberId);

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

        if(await dbContext.VillaNumbers.AnyAsync(v => v.Villa_Number == villaNumber.Villa_Number))
        {
            TempData["error"] = "Villa Number with same number already exists";
		    return RedirectToAction(nameof(Index));
        }

        dbContext.VillaNumbers.Update(villaNumber);
        await dbContext.SaveChangesAsync();

        TempData["success"] = "Villa Number updated successfully.";

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int villaNumberId)
    {
        var villaNumber = await dbContext.VillaNumbers.FirstOrDefaultAsync(v => v.Villa_Number == villaNumberId);

        if(villaNumber is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

        var villaList = await dbContext.Villas.ToListAsync();

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

        var villaNumberToDelete = await dbContext.VillaNumbers.FirstOrDefaultAsync(v => v.Villa_Number == villaNumberViewModel.VillaNumber.Villa_Number);

		if (villaNumberToDelete is not null)
        {
			dbContext.VillaNumbers.Remove(villaNumberToDelete);
			await dbContext.SaveChangesAsync();

            TempData["success"] = "Villa deleted successfully.";
		}

        TempData["error"] = "Villa not found.";

	    return RedirectToAction(nameof(Index));
    }
}