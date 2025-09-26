using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces;

public interface IBookingService
{
	Task CreateBookingAsync(Booking booking);

	Task<Booking?> GetBookingByIdAsync(int id);

	Task<List<Booking>> GetAllBookingsAsync(string userId = "", string? status = "");

	Task UpdateStatusAsync(int bookingId, string bookingStatus, int villaNumber = 0);

	Task UpdateStripePaymentIDAsync(int bookingId, string sessionId, string paymentId);

	Task<IEnumerable<int>> GetCheckedInVillaNumbers(int villaId);
}