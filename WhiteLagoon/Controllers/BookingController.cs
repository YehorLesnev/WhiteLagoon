using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Controllers;

public class BookingController(IUnitOfWork unitOfWork) : Controller
{
	[Authorize]
	public async Task<IActionResult> FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
	{
		var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
		var userId = claimsIdentity?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

		var user = await unitOfWork.Users.GetAsync(u => u.Id == userId);

		if(user is null)
		{
			return RedirectToAction(nameof(AccountController.Login), "Account");
		}	

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
			TotalCost = villa.Price * nights,
			UserId = user.Id,
			PhoneNumber = user.PhoneNumber,
			Email = user.Email,
			Name = user.Name,
		};

		return View(booking);
	}

	[Authorize]
	[HttpPost]
	public async Task<IActionResult> FinalizeBooking(Booking booking)
	{
		var villa = await unitOfWork.Villas.GetAsync(v => v.Id == booking.VillaId, includeProperties: nameof(Villa.VillaAmenities));

		if(villa is null)
		{
			return NotFound();
		}

		booking.TotalCost = villa.Price * booking.Nights;
		booking.Status = BookingStatusConstants.Pending;

		await unitOfWork.Bookings.AddAsync(booking);
		await unitOfWork.SaveAsync();

		return RedirectToAction(nameof(BookingConfirmation), new { bookingId = booking.Id });
	}

	[Authorize]
	public async Task<IActionResult> BookingConfirmation(int bookingId)
	{
		return View(bookingId);
	}
}