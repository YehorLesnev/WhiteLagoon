namespace WhiteLagoon.ViewModels;

public class RadialBarChartViewModel
{
	public decimal TotalCount { get; set; }

	public decimal CurrentMonthCount { get; set; }

	public int IncreaseDecreaseAmount { get; set; }

	public bool HasRatioIncreased { get; set; }

	public int[] Series { get; set; }
}