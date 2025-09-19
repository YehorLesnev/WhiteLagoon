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

    [HttpPost]
	public async Task<IActionResult> Index(HomeViewModel homeViewModel)
    {
        homeViewModel.VillaList ??= (await unitOfWork.Villas.GetAllAsync(includeProperties: nameof(Villa.VillaAmenities))) ?? [];

		foreach (var villa in homeViewModel?.VillaList ?? [])
        {
            if(villa.Id % 2 == 0)
            {
                villa.IsAvailable = false;
            }
		}

		return View(homeViewModel);
    }

    [HttpPost]
	public async Task<IActionResult> GetVillasByDate(int nights, DateOnly checkInDate)
    {
        var villaList = await unitOfWork.Villas.GetAllAsync(includeProperties: nameof(Villa.VillaAmenities));

		foreach (var villa in villaList ?? [])
        {
            if(villa.Id % 2 == 0)
            {
                villa.IsAvailable = false;
            }
		}

        var homeViewModel = new HomeViewModel
        {
            VillaList = villaList,
            Nights = nights,
            CheckInDate = checkInDate
		};

		return PartialView("_VillaList", homeViewModel);
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
