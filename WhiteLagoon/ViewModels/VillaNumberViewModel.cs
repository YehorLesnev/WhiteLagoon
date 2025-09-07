using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.ViewModels;

public class VillaNumberViewModel
{
	public VillaNumber? VillaNumber { get; set; }

	public List<SelectListItem>? VillaList { get; set; }
}