using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
	void Update(Booking entity);

	Task UpdateStatusAsync(int bookingId, string bookingStatus);

	Task UpdateStripePaymentIDAsync(int bookingId, string sessionId, string paymentId);
}