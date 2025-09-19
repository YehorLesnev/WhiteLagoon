using WhiteLagoon.Application.Common.Interfaces;
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
}