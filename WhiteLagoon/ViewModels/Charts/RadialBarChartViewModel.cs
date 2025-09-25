namespace WhiteLagoon.ViewModels.Charts;

public class RadialBarChartViewModel
{
	public double TotalCount { get; set; }

	public double CurrentMonthCount { get; set; }

	public double IncreaseDecreaseAmount { get; set; }

	public bool HasRatioIncreased { get; set; }

	public int[] Series { get; set; }
}