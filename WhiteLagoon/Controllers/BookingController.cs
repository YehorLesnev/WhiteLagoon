using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Controllers;

public class BookingController(IUnitOfWork unitOfWork) : Controller
{
	public async Task<IActionResult> FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
	{
		var villa = await unitOfWork.Villas.GetAsync(v => v.Id == villaId, includeProperties: nameof(Villa.VillaAmenities));

		if(villa is null)
		{
			return NotFound();
		}

		var booking = new Booking
		{
			VillaId = villaId,
			Villa = villa,
			CheckInDate = checkInDate,
			Nights = nights,
			CheckOutDate = checkInDate.AddDays(nights),
			TotalCost = villa.Price * nights
		};

		return View(booking);
	}
}
