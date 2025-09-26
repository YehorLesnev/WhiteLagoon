using WhiteLagoon.ViewModels.Charts;

namespace WhiteLagoon.Application.Services.Interfaces;

public interface IDashboardService
{
	Task<RadialBarChartDto> GetBookingRadialChartData();

	Task<RadialBarChartDto> GetRegisteredUserChartData();

	Task<RadialBarChartDto> GetRevenueChartData();

	Task<PieChartDto> GetBookingPieChartData();

	Task<LineChartDto> GetMemberAndBookingLineChartData();
}