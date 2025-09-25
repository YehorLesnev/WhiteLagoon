using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility.Constants;

namespace WhiteLagoon.Controllers;

public class DashboardController(IUnitOfWork unitOfWork) : Controller
{
	private static readonly int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;

	private readonly DateTime previousMonthStartDate = new(DateTime.Now.Month == 1 ? DateTime.Now.Year - 1 : DateTime.Now.Year, previousMonth, 1);

	private readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

	public async Task<IActionResult> Index()
	{
		return View();
	}

	public async Task<IActionResult> GetBookingRadialChartData()
	{
		var totalBookings = await unitOfWork.Bookings.GetAllAsync(b => b.Status != BookingStatusConstants.Cancelled);
		var countByCurrentMonth = totalBookings.Count(b => b.BookingDate >= currentMonthStartDate && b.BookingDate <= DateTime.Now);
		var countByPreviousMonth = totalBookings.Count(b => b.BookingDate >= previousMonthStartDate && b.BookingDate <= currentMonthStartDate);

		var radialBarChartViewModel = new ViewModels.RadialBarChartViewModel
		{
			TotalCount = totalBookings.Count,
			CurrentMonthCount = countByCurrentMonth,
			IncreaseDecreaseAmount = Math.Abs(countByCurrentMonth - countByPreviousMonth),
			HasRatioIncreased = countByCurrentMonth >= countByPreviousMonth,
		};

		int increaseDecreaseRatio = 100;

		if (countByPreviousMonth != 0)
		{
			increaseDecreaseRatio = (int)Math.Round((decimal)Math.Abs(countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100, 2);
		}

		radialBarChartViewModel.IncreaseDecreaseAmount = increaseDecreaseRatio;
		radialBarChartViewModel.HasRatioIncreased = currentMonthStartDate > previousMonthStartDate;
		radialBarChartViewModel.Series = [increaseDecreaseRatio];

		return Json(radialBarChartViewModel);
	}

}