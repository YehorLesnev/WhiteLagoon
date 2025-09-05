using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Controllers;

public class VillasController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var villas = await dbContext.Villas.ToListAsync();

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

			await dbContext.Villas.AddAsync(villa);
        await dbContext.SaveChangesAsync();

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int villaId)
    {
        Villa? villa = await dbContext.Villas.FirstOrDefaultAsync(v => v.Id == villaId);

        if(villa is null)
			return NotFound();

		return View(villa);
    }
    
    [HttpPost]
	public async Task<IActionResult> Update(Villa villa)
    {
        if(villa is null)
			return NotFound();

		return View(villa);
    }
}