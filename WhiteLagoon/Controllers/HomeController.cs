using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.ViewModels;

namespace WhiteLagoon.Controllers;

public class HomeController(IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await GetHomeViewModelAsync());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }

    private async Task<HomeViewModel> GetHomeViewModelAsync()
    {
        var homeViewModel = new HomeViewModel
        {
            VillaList = await unitOfWork.Villas.GetAllAsync(includeProperties: nameof(Villa.VillaAmenities)),
            Nights = 1,
            CheckInDate = DateOnly.FromDateTime(DateTime.Now)
		};

        return homeViewModel;
	}
}
