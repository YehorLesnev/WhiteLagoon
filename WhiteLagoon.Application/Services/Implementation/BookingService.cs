using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation;

public class BookingService(IUnitOfWork unitOfWork) : IBookingService
{
	public async Task CreateBookingAsync(Booking booking)
	{
		await unitOfWork.Bookings.AddAsync(booking);
		await unitOfWork.SaveAsync();
	}

	public async Task<List<Booking>> GetAllBookingsAsync(string userId = "", string? statusListSeparatedByComma = "")
	{
		IEnumerable<string> statusList = statusListSeparatedByComma?.ToLower()?.Split(',') ?? [];

		if (statusList.Any())
		{
			if (string.IsNullOrEmpty(userId))
			{
				return await unitOfWork.Bookings.GetAllAsync(
					filter: b => statusList.Contains(b.Status.ToLower()),
					includeProperties: $"{nameof(Booking.Villa)},{nameof(Booking.User)}");
			}
			else
			{
				return await unitOfWork.Bookings.GetAllAsync(
					filter: b => b.UserId == userId
						&& statusList.Contains(b.Status.ToLower()),
					includeProperties: $"{nameof(Booking.Villa)},{nameof(Booking.User)}");
			}
		}

		return await unitOfWork.Bookings.GetAllAsync(
			filter: b => b.UserId == userId,
			includeProperties: $"{nameof(Booking.Villa)},{nameof(Booking.User)}");
	}

	public async Task<Booking?> GetBookingByIdAsync(int id)
	{
		return await unitOfWork.Bookings.GetAsync(
			filter: b => b.Id == id,
			includeProperties: $"{nameof(Booking.Villa)},{nameof(Booking.User)}");
	}

	public async Task<IEnumerable<int>> GetCheckedInVillaNumbers(int villaId)
	{
		return (await unitOfWork.Bookings.GetAllAsync(x => x.VillaId == villaId && x.Status == BookingStatusConstants.CheckedIn))
					.Select(x => x.VillaNumber);
	}

	public async Task UpdateStatusAsync(int bookingId, string bookingStatus, int villaNumber = 0)
	{
		var booking = await unitOfWork.Bookings.GetAsync(b => b.Id == bookingId);

		if (booking is null)
			return;

		booking.Status = bookingStatus;

		if (bookingStatus.Equals(BookingStatusConstants.CheckedIn, StringComparison.InvariantCultureIgnoreCase))
		{
			booking.VillaNumber = villaNumber;
			booking.ActualCheckInDate = DateTime.Now;
		}

		if (bookingStatus.Equals(BookingStatusConstants.Completed, StringComparison.InvariantCultureIgnoreCase))
		{
			booking.ActualCheckOutDate = DateTime.Now;
		}

		await unitOfWork.SaveAsync();
	}

	public async Task UpdateStripePaymentIDAsync(int bookingId, string sessionId, string paymentId)
	{
		var booking = await unitOfWork.Bookings.GetAsync(b => b.Id == bookingId);

		if (booking is null)
			return;

		if (!string.IsNullOrEmpty(sessionId))
		{
			booking.StripeSessionId = sessionId;
		}

		if (!string.IsNullOrEmpty(paymentId))
		{
			booking.StripeSessionId = paymentId;
			booking.PaymentDate = DateTime.Now;
			booking.IsPaymentSuccessful = true;
		}

		await unitOfWork.SaveAsync();
	}
}