using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class BookingRepository(ApplicationDbContext dbContext)
	: Repository<Booking>(dbContext), IBookingRepository
{
	public void Update(Booking entity)
	{
		dbSet.Update(entity);
	}

	public async Task UpdateStatusAsync(int bookingId, string bookingStatus)
	{
		var booking = await dbSet.FirstOrDefaultAsync(b => b.Id == bookingId);

		if (booking is null)
			return;

		booking.Status = bookingStatus;

		if (bookingStatus.Equals(BookingStatusConstants.CheckedIn, StringComparison.InvariantCultureIgnoreCase))
		{
			booking.ActualCheckInDate = DateTime.Now;
		}

		if (bookingStatus.Equals(BookingStatusConstants.Completed, StringComparison.InvariantCultureIgnoreCase))
		{
			booking.ActualCheckOutDate = DateTime.Now;
		}
	}

	public async Task UpdateStripePaymentIDAsync(int bookingId, string sessionId, string paymentId)
	{
		var booking = await dbSet.FirstOrDefaultAsync(b => b.Id == bookingId);

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
	}
}