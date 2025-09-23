using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Utility.Helpers;

public static class VillaRoomsAvailabilityHelper
{
	public static int GetNumberOfAvailableRooms(
		int villaId,
		List<VillaNumber> villaNumbers,
		DateOnly checkInDate,
		int nights,
		List<Booking> bookings)
	{
		List<int> bookingInDate = [];
		int finalAvailableRooms = int.MaxValue;
		var numOfroomsInVilla = villaNumbers.Where(x => x.VillaId == villaId).Count();

		for (int i = 0; i < nights; ++i)
		{
			var villasBooked = bookings.Where(x => x.CheckInDate <= checkInDate.AddDays(i) &&
												x.CheckOutDate > checkInDate.AddDays(i) &&
												x.VillaId == villaId);

			foreach (Booking booking in villasBooked)
			{
				if (!bookingInDate.Contains(booking.Id))
				{
					bookingInDate.Add(booking.Id);
				}
			}

			var totalAvailableRooms = numOfroomsInVilla - bookingInDate.Count;
			if (totalAvailableRooms == 0)
			{
				return 0;
			}
			else
			{
				if (finalAvailableRooms > totalAvailableRooms)
				{
					finalAvailableRooms = totalAvailableRooms;
				}
			}
		}

		return finalAvailableRooms;
	}
}