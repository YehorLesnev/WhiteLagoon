using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Application.Utility.Helpers;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Controllers;

public class BookingController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) : Controller
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

		if (user is null)
		{
			return RedirectToAction(nameof(AccountController.Login), "Account");
		}

		var villa = await unitOfWork.Villas.GetAsync(v => v.Id == villaId, includeProperties: nameof(Villa.VillaAmenities));

		if (villa is null)
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
			BookingDate = DateTime.Now,
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

		if (villa is null)
		{
			return NotFound();
		}

		booking.TotalCost = villa.Price * booking.Nights;
		booking.Status = BookingStatusConstants.Pending;

		var villaList = await unitOfWork.Villas.GetAllAsync(includeProperties: nameof(Villa.VillaAmenities));
		var villaNumbers = await unitOfWork.VillaNumbers.GetAllAsync();
		var bookings = await unitOfWork.Bookings.GetAllAsync(b =>
			b.Status == BookingStatusConstants.Approved || b.Status == BookingStatusConstants.CheckedIn);

		int roomsAvailable = VillaRoomsAvailabilityHelper.GetNumberOfAvailableRooms(villa.Id, villaNumbers, booking.CheckInDate, booking.Nights, bookings);

		if(roomsAvailable <= 0)
		{
			TempData["error"] = "Rooms has been sold out! Please choose different dates or a different villa.";
			return RedirectToAction(nameof(FinalizeBooking), new { villaId = booking.VillaId, checkInDate = booking.CheckInDate, nights = booking.Nights });
		}

		villa.IsAvailable = roomsAvailable > 0;

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
			CancelUrl = $"{domain}/Booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
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

		if (booking is not null && booking.Status.Equals(BookingStatusConstants.Pending, StringComparison.InvariantCultureIgnoreCase))
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

	[Authorize]
	public async Task<IActionResult> Details(int bookingId)
	{
		var booking = await unitOfWork.Bookings.GetAsync(b => b.Id == bookingId,
									includeProperties: $"{nameof(Booking.User)},{nameof(Booking.Villa)}");

		if (booking is null)
			return NotFound();

		if(booking?.VillaNumber == 0 && booking.Status == BookingStatusConstants.Approved)
		{
			var availableVillaNumbers = await AssignAvailableVillaNumberByVilla(booking.VillaId);
			booking.VillaNumbers = await unitOfWork.VillaNumbers.GetAllAsync(x => x.VillaId == booking.VillaId &&
																				availableVillaNumbers.Contains(x.Villa_Number));
		}

		return View(booking);
	}

	[HttpPost]
	[Authorize(Roles = RolesConstants.Admin)]
	public async Task<IActionResult> CheckIn(Booking booking)
	{
		await unitOfWork.Bookings.UpdateStatusAsync(booking.Id, BookingStatusConstants.CheckedIn, booking.VillaNumber);
		await unitOfWork.SaveAsync();

		TempData["success"] = "Booking checked in successfully.";

		return RedirectToAction(nameof(Details), new { bookingId = booking.Id });
	}

	[HttpPost]
	[Authorize(Roles = RolesConstants.Admin)]
	public async Task<IActionResult> CheckOut(Booking booking)
	{
		await unitOfWork.Bookings.UpdateStatusAsync(booking.Id, BookingStatusConstants.Completed, booking.VillaNumber);
		await unitOfWork.SaveAsync();

		TempData["success"] = "Booking checked out successfully.";

		return RedirectToAction(nameof(Details), new { bookingId = booking.Id });
	}

	[HttpPost]
	[Authorize(Roles = RolesConstants.Admin)]
	public async Task<IActionResult> CancelBooking(Booking booking)
	{
		await unitOfWork.Bookings.UpdateStatusAsync(booking.Id, BookingStatusConstants.Cancelled, booking.VillaNumber);
		await unitOfWork.SaveAsync();

		TempData["success"] = "Booking cancelled successfully.";

		return RedirectToAction(nameof(Details), new { bookingId = booking.Id });
	}

	[HttpPost]
	[Authorize(Roles = RolesConstants.Admin)]
	public async Task<IActionResult> GenerateInvoice(int id)
	{
		string basePath = webHostEnvironment.WebRootPath;

		WordDocument document = new();
		string dataPath = Path.Combine(basePath, "exports", "BookingDetails.docx");

		using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		document.Open(fileStream, Syncfusion.DocIO.FormatType.Automatic);

		var booking = await unitOfWork.Bookings.GetAsync(b => b.Id == id,
									includeProperties: $"{nameof(Booking.User)},{nameof(Booking.Villa)}");

		if (booking is null)
			return NotFound();

		TextSelection textSelection = document.Find("xx_customer_name", false, true);
		WTextRange textRange = textSelection.GetAsOneRange();
		textRange.Text = booking.Name;

		textSelection = document.Find("XX_BOOKING_NUMBER", false, true);
		textRange = textSelection.GetAsOneRange();
		textRange.Text = $"BOOKING ID - {booking.Id}";

		textSelection = document.Find("XX_BOOKING_DATE", false, true);
		textRange = textSelection.GetAsOneRange();
		textRange.Text = $"BOOKING DATE - {booking.BookingDate}";

		textSelection = document.Find("xx_customer_phone", false, true);
		textRange = textSelection.GetAsOneRange();
		textRange.Text = booking.PhoneNumber;

		textSelection = document.Find("xx_payment_date", false, true);
		textRange = textSelection.GetAsOneRange();
		textRange.Text = booking.PaymentDate.ToString();

		textSelection = document.Find("xx_checkin_date", false, true);
		textRange = textSelection.GetAsOneRange();
		textRange.Text = booking.CheckInDate.ToString();

		textSelection = document.Find("xx_checkout_date", false, true);
		textRange = textSelection.GetAsOneRange();
		textRange.Text = booking.CheckOutDate.ToString();

		textSelection = document.Find("xx_booking_total", false, true);
		textRange = textSelection.GetAsOneRange();
		textRange.Text = booking.TotalCost.ToString("c");

		using DocIORenderer renderer = new();

		MemoryStream ms = new();
		document.Save(ms, Syncfusion.DocIO.FormatType.Docx);
		ms.Position = 0;

		return File(ms, "application/docx", "BookingDetails.docx");
	}

	private async Task<List<int>> AssignAvailableVillaNumberByVilla(int villaId)
	{
		List<int> availableVillaNumbers = [];
		var villaNumbers = await unitOfWork.VillaNumbers.GetAllAsync(vn => vn.VillaId == villaId);

		var checkedInVilla = (await unitOfWork.Bookings.GetAllAsync(x => x.VillaId == villaId &&
																		x.Status == BookingStatusConstants.CheckedIn))
													.Select(x => x.VillaNumber);
		foreach (var villaNumber in villaNumbers ?? [])
		{
			if (!checkedInVilla.Contains(villaNumber.Villa_Number))
			{
				availableVillaNumbers.Add(villaNumber.Villa_Number);
			}
		}

		return availableVillaNumbers;
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

		if (!string.IsNullOrEmpty(status))
		{
			bookings = bookings.Where(b => b.Status.Equals(status, StringComparison.InvariantCultureIgnoreCase));
		}

		return Json(new { data = bookings });
	}

	#endregion
}