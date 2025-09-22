using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Linq.Expressions;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Controllers;

public class BookingController(IUnitOfWork unitOfWork) : Controller
{
	[Authorize]
	public IActionResult Index()
	{
		return View();
	}	

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

		var domain = $"{Request.Scheme}://{Request.Host.Value}";
		var options = new SessionCreateOptions
		{
			LineItems = new List<SessionLineItemOptions>()
			{
				new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions          
					{
						UnitAmount = (long)(booking.TotalCost * 100),
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = villa.Name,
							Description = villa.Description,
							//Images = [$"{domain}/{villa.ImageUrl}"],
						},
					},
					Quantity = 1,
				},
			},
			Mode = "payment",
			SuccessUrl = $"{domain}/Booking/BookingConfirmation?bookingId={booking.Id}",
			CancelUrl =  $"{domain}/Booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
		};

		var service = new SessionService();
		Session session = service.Create(options);

		await unitOfWork.Bookings.UpdateStripePaymentIDAsync(booking.Id, session.Id, session.PaymentIntentId);
		await unitOfWork.SaveAsync();	

		Response.Headers.Add("Location", session.Url);

		return new StatusCodeResult(303);
	}

	[Authorize]
	public async Task<IActionResult> BookingConfirmation(int bookingId)
	{
		var booking = await unitOfWork.Bookings.GetAsync(b => b.Id == bookingId, includeProperties: $"{nameof(Booking.User)},{nameof(Booking.Villa)}");

		if(booking is not null && booking.Status.Equals(BookingStatusConstants.Pending, StringComparison.InvariantCultureIgnoreCase))
		{
			var service = new SessionService();
			var session = service.Get(booking.StripeSessionId);
			
			if (session.PaymentStatus == "paid")
			{ 
				await unitOfWork.Bookings.UpdateStatusAsync(booking.Id, BookingStatusConstants.Approved);
				await unitOfWork.Bookings.UpdateStripePaymentIDAsync(booking.Id, session.Id, session.PaymentIntentId);
				await unitOfWork.SaveAsync();
			}
		}

		return View(bookingId);
	}

	#region API Calls

	[HttpGet]
	[Authorize]
	public async Task<IActionResult> GetAll(string? status = null)
	{
		IEnumerable<Booking> bookings;
		var isAdmin = User.IsInRole(RolesConstants.Admin);

		if (isAdmin)
		{
			bookings = await unitOfWork.Bookings
				.GetAllAsync(includeProperties: $"{nameof(Booking.User)},{nameof(Booking.Villa)}");
		}
		else
		{
			var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
			var userId = claimsIdentity?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			bookings = await unitOfWork.Bookings
				.GetAllAsync(b => b.UserId == userId, 
							includeProperties: $"{nameof(Booking.User)},{nameof(Booking.Villa)}");
		}

		if(!string.IsNullOrEmpty(status))
		{
			bookings = bookings.Where(b => b.Status.Equals(status, StringComparison.InvariantCultureIgnoreCase));
		}

		return Json(new { data = bookings });
	}

	#endregion
}