using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.ViewModels;

public class AmenityViewModel
{
	public Amenity? Amenity { get; set; }

	public List<SelectListItem>? VillaList { get; set; }
}