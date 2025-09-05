using System.ComponentModel.DataAnnotations;

namespace WhiteLagoon.Domain.Entities;

public class Villa
{
	public int Id { get; set; }

	[MaxLength(200)]
	public required string Name { get; set; }

	public string? Description { get; set; }

	[Display(Name = "Price Per Night")]
	[Range(1, 100000)]
	public double Price { get; set; }

	[Display(Name = "Sq. Ft")]
	public int Sqft { get; set; }

	[Range(1, 20)]
	public int Occupancy { get; set; }

	[Display(Name = "Image Url")]
	public string? ImageUrl { get; set; }

	public DateTime? CreatedDate { get; set; }

	public DateTime? UpdatedDate { get; set; }
}