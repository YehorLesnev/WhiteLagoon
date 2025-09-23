using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.ViewModels;
using WhiteLagoon.Application.Utility.Helpers;
using WhiteLagoon.Application.Utility.Constants;

namespace WhiteLagoon.Controllers;

public class HomeController(IUnitOfWork unitOfWork) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await GetHomeViewModelAsync());
    }

    [HttpPost]
	public async Task<IActionResult> GetVillasByDate(int nights, DateOnly checkInDate)
    {
        var villaList = await unitOfWork.Villas.GetAllAsync(includeProperties: nameof(Villa.VillaAmenities));
        var villaNumbers = await unitOfWork.VillaNumbers.GetAllAsync();
        var bookings = await unitOfWork.Bookings.GetAllAsync(b => 
            b.Status == BookingStatusConstants.Approved || b.Status == BookingStatusConstants.CheckedIn);
		
        foreach (var villa in villaList ?? [])
        {
            int roomsAvailable = VillaRoomsAvailabilityHelper.GetNumberOfAvailableRooms(villa.Id, villaNumbers, checkInDate, nights, bookings);
		
            villa.IsAvailable = roomsAvailable > 0;
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
