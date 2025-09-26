using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
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
	public async Task<IActionResult> GenerateInvoice(int id, string downloadType)
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

		WTable table = new(document);
		table.TableFormat.Borders.LineWidth = 1f;
		table.TableFormat.Borders.Horizontal.LineWidth = 1f;
		table.TableFormat.Borders.Color = Syncfusion.Drawing.Color.Black;
		table.TableFormat.IsAutoResized = true;
		table.TableFormat.Paddings.Top = 7f;
		table.TableFormat.Paddings.Bottom = 7f;

		int rows = booking.VillaNumber >0 ? 3 : 2;
		table.ResetCells(rows, 4);

		WTableRow row0 = table.Rows[0];
		row0.Cells[0].AddParagraph().AppendText("NIGHTS");
		row0.Cells[0].Width = 80;
		row0.Cells[1].AddParagraph().AppendText("VILLA");
		row0.Cells[1].Width = 220;
		row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");
		row0.Cells[3].AddParagraph().AppendText("TOTAL");
		row0.Cells[3].Width = 80;

		WTableRow row = table.Rows[1];
		row.Cells[0].AddParagraph().AppendText(booking.Nights.ToString());
		row.Cells[0].Width = 80;
		row.Cells[1].AddParagraph().AppendText(booking.Villa.Name);
		row.Cells[1].Width = 220;
		row.Cells[2].AddParagraph().AppendText(booking.Villa.Price.ToString("c"));
		row.Cells[3].AddParagraph().AppendText(booking.TotalCost.ToString("c"));
		row.Cells[3].Width = 80;

		if(booking.VillaNumber > 0)
		{
			WTableRow row2 = table.Rows[2];
			row2.Cells[0].AddParagraph().AppendText($"VILLA NUMBER");
			row2.Cells[0].Width = 80;
			row2.Cells[1].AddParagraph().AppendText(booking.VillaNumber.ToString());
			row2.Cells[1].Width = 220;
			row2.Cells[3].Width = 80;
		}

		var customStyleName = "TableStyle";
		WTableStyle tableStyle = document.AddTableStyle(customStyleName) as WTableStyle;
		tableStyle.TableProperties.ColumnStripe = 2;
		tableStyle.TableProperties.Paddings.Top = 2;
		tableStyle.TableProperties.Paddings.Bottom = 1;
		tableStyle.TableProperties.Paddings.Left = 5.4f;
		tableStyle.TableProperties.Paddings.Right = 5.4f;

		ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow) as ConditionalFormattingStyle;
		firstRowStyle.CharacterFormat.Bold = true;
		firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
		firstRowStyle.CellProperties.BackColor = Color.Black;

		table.ApplyStyle(customStyleName);

		TextBodyPart tableBodyPart = new(document);
		tableBodyPart.BodyItems.Add(table);

		document.Replace("<ADDTABLEHERE>", tableBodyPart, false, true);

		using DocIORenderer renderer = new();
		MemoryStream ms = new();

		switch(downloadType)
		{
			case nameof(DocumentType.Word):
				document.Save(ms, Syncfusion.DocIO.FormatType.Docx);
				ms.Position = 0;

				return File(ms, "application/docx", "BookingDetails.docx");
			case nameof(DocumentType.Pdf):
				PdfDocument pdfDocument = renderer.ConvertToPDF(document);
				pdfDocument.Save(ms);
				ms.Position = 0;

				return File(ms, "application/pdf", "BookingDetails.pdf");
			default:
				return BadRequest("Invalid document type.");
		}
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