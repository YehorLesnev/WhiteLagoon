using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhiteLagoon.Domain.Entities;

public class Amenity
{
	[Key]
	public int Id { get; set; }

	[Display(Name = "Amenity Name")]
	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; } = string.Empty;
	
	[ForeignKey("Villa")]
	public int VillaId { get; set; }

	[ValidateNever]
	public Villa Villa { get; set; }
}
