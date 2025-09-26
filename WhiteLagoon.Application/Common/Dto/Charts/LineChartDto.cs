namespace WhiteLagoon.ViewModels.Charts;

public class LineChartDto
{
	public List<ChartDatePoint> Series { get; set; }

	public string[] Categories { get; set; }
}

public class ChartDatePoint
{
	public string Name { get; set; }

	public int[] Data { get; set; }
}