using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interfaces;

namespace WhiteLagoon.Controllers;

public class DashboardController(IDashboardService dashboardService) : Controller
{
	public async Task<IActionResult> Index()
	{
		return View();
	}

	public async Task<IActionResult> GetBookingRadialChartData()
	{

		var radialBarChartViewModel = await dashboardService.GetBookingRadialChartData();

		return Json(radialBarChartViewModel);
	}

	public async Task<IActionResult> GetRegisteredUserChartData()
	{
		var radialBarChartViewModel = await dashboardService.GetRegisteredUserChartData();

		return Json(radialBarChartViewModel);
	}

	public async Task<IActionResult> GetRevenueChartData()
	{
		var radialBarChartViewModel = await dashboardService.GetRevenueChartData();

		return Json(radialBarChartViewModel);
	}

	public async Task<IActionResult> GetBookingPieChartData()
	{
		var pieChartViewModel = await dashboardService.GetBookingPieChartData();

		return Json(pieChartViewModel);
	}

	public async Task<IActionResult> GetMemberAndBookingLineChartData()
	{

		var lineChartViewModel = await dashboardService.GetMemberAndBookingLineChartData();

		return Json(lineChartViewModel);
	}
}