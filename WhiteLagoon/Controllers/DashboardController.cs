using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.ViewModels.Charts;

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
		var totalBookings = await unitOfWork.Bookings.GetAllAsync(b => b.Status != BookingStatusConstants.Cancelled && b.Status != BookingStatusConstants.Pending);
		var countByCurrentMonth = totalBookings.Count(b => b.BookingDate >= currentMonthStartDate && b.BookingDate <= DateTime.Now);
		var countByPreviousMonth = totalBookings.Count(b => b.BookingDate >= previousMonthStartDate && b.BookingDate <= currentMonthStartDate);

		var radialBarChartViewModel = GetRadialBarChartViewModel(totalBookings.Count, countByCurrentMonth, countByPreviousMonth);

		return Json(radialBarChartViewModel);
	}

	public async Task<IActionResult> GetRegisteredUserChartData()
	{
		var totalUsers = await unitOfWork.Users.GetAllAsync();
		var countByCurrentMonth = totalUsers.Count(b => b.CreatedAt >= currentMonthStartDate && b.CreatedAt <= DateTime.Now);
		var countByPreviousMonth = totalUsers.Count(b => b.CreatedAt >= previousMonthStartDate && b.CreatedAt <= currentMonthStartDate);

		var radialBarChartViewModel = GetRadialBarChartViewModel(totalUsers.Count, countByCurrentMonth, countByPreviousMonth);

		return Json(radialBarChartViewModel);
	}

	public async Task<IActionResult> GetRevenueChartData()
	{
		var totalBookings = await unitOfWork.Bookings.GetAllAsync(b => b.Status != BookingStatusConstants.Cancelled && b.Status != BookingStatusConstants.Pending);
		var totalRevenue = Convert.ToInt32(totalBookings.Sum(b => b.TotalCost));
		var countByCurrentMonth = totalBookings.Where(b => b.BookingDate >= currentMonthStartDate && b.BookingDate <= DateTime.Now).Sum(b => b.TotalCost);
		var countByPreviousMonth = totalBookings.Where(b => b.BookingDate >= previousMonthStartDate && b.BookingDate <= currentMonthStartDate).Sum(b => b.TotalCost);

		var radialBarChartViewModel = GetRadialBarChartViewModel(totalBookings.Count, countByCurrentMonth, countByPreviousMonth);

		return Json(radialBarChartViewModel);
	}

	public async Task<IActionResult> GetBookingPieChartData()
	{
		var totalBookings = await unitOfWork.Bookings.GetAllAsync(b => b.BookingDate >= DateTime.Now.AddDays(-30) &&
			(b.Status != BookingStatusConstants.Cancelled && b.Status != BookingStatusConstants.Pending));

		var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(g => g.Count() == 1).Select(x => x.Key).ToList();
		int bookingsByNewCustomer = customerWithOneBooking.Count;
		int bookingsByReturningCustomer = totalBookings.Count - bookingsByNewCustomer;

		var pieChartViewModel = new PieChartViewModel
		{
			Series = [bookingsByNewCustomer, bookingsByReturningCustomer],
			Labels = ["New Customer", "Returning Customer"],
		};

		return Json(pieChartViewModel);
	}

	private static RadialBarChartViewModel GetRadialBarChartViewModel(int totalCount, double currentMonthCount, double previousMonthCount)
	{
		var radialBarChartViewModel = new RadialBarChartViewModel
		{
			TotalCount = totalCount,
			CurrentMonthCount = currentMonthCount,
			IncreaseDecreaseAmount = Math.Abs(currentMonthCount - previousMonthCount),
			HasRatioIncreased = currentMonthCount >= previousMonthCount,
		};
		int increaseDecreaseRatio = 100;
		if (previousMonthCount != 0)
		{
			increaseDecreaseRatio = (int)Math.Round(Math.Abs(currentMonthCount - previousMonthCount) / previousMonthCount * 100, 2);
		}
		radialBarChartViewModel.IncreaseDecreaseAmount = increaseDecreaseRatio;
		radialBarChartViewModel.HasRatioIncreased = currentMonthCount > previousMonthCount;
		radialBarChartViewModel.Series = [increaseDecreaseRatio];
		
		return radialBarChartViewModel;
	}
}