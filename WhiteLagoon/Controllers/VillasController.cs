using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class VillasController(
    IUnitOfWork unitOfWork,
    IWebHostEnvironment webHostEnvironment) : Controller
{
    public async Task<IActionResult> Index()
    {
        var villas = await unitOfWork.Villas.GetAllAsync();

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

        if(villa.Image is not null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
			string imagePath = Path.Combine(webHostEnvironment.WebRootPath, @"images\VillaImage", fileName);
		
            using var fileStream = new FileStream(imagePath, FileMode.Create);
            await villa.Image.CopyToAsync(fileStream);

            villa.ImageUrl = $@"\images\VillaImage\{fileName}";
		}
        else
        {
            villa.ImageUrl = "https://placehold.co/600x400";
		}

	    await unitOfWork.Villas.AddAsync(villa);
        await unitOfWork.SaveAsync();

        TempData["success"] = "Villa created successfully.";    

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int villaId)
    {
        Villa? villa = await unitOfWork.Villas.GetAsync(v => v.Id == villaId);

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

		if (villa.Image is not null)
		{
            if(!string.IsNullOrEmpty(villa.ImageUrl))
            {
                string oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
			}

			string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
			string imagePath = Path.Combine(webHostEnvironment.WebRootPath, @"images\VillaImage", fileName);

			using var fileStream = new FileStream(imagePath, FileMode.Create);
			await villa.Image.CopyToAsync(fileStream);

			villa.ImageUrl = $@"\images\VillaImage\{fileName}";
		}
	
		unitOfWork.Villas.Update(villa);
        await unitOfWork.SaveAsync();

        TempData["success"] = "Villa updated successfully.";

		return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int villaId)
    {
        Villa? villa = await unitOfWork.Villas.GetAsync(v => v.Id == villaId);
        
        if(villa is null)
			return RedirectToAction(nameof(HomeController.Error), "Home");

		return View(villa);
    }

    [HttpPost]
	public async Task<IActionResult> Delete(Villa villa)
    {
        var villaToDelete = await unitOfWork.Villas.GetAsync(v => v.Id == villa.Id);
        
		if (villaToDelete is not null)
        {
		    unitOfWork.Villas.Remove(villaToDelete);
			await unitOfWork.SaveAsync();

            if(!string.IsNullOrEmpty(villaToDelete.ImageUrl))
            {
                string imagePath = Path.Combine(webHostEnvironment.WebRootPath, villaToDelete.ImageUrl.TrimStart('\\'));
                
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
			}

            TempData["success"] = "Villa deleted successfully.";

			return RedirectToAction(nameof(Index));
		}

        TempData["error"] = "Villa not found.";

		return View();
    }
}