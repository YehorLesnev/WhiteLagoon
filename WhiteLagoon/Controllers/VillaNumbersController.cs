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

    public async Task<IActionResult> Update(int villaId)
    {
        Villa? villa = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == villaId);

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

        dbContext.Villas.Update(villa);
        await dbContext.SaveChangesAsync();

        TempData["success"] = "Villa updated successfully.";

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int villaId)
    {
        Villa? villa = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == villaId);

        if(villa is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		return View(villa);
    }

    [HttpPost]
	public async Task<IActionResult> Delete(Villa villa)
    {
        var villaToDelete = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == villa.Id);

		if (villaToDelete is not null)
        {
			dbContext.Villas.Remove(villaToDelete);
			await dbContext.SaveChangesAsync();

            TempData["success"] = "Villa deleted successfully.";

			return RedirectToAction(nameof(Index));
		}

        TempData["error"] = "Villa not found.";

		return View();
    }
}