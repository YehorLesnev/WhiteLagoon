using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
	public IVillaRepository Villas => new VillaRepository(dbContext);

	public IVillaNumberRepository VillaNumbers => new VillaNumberRepository(dbContext);

	public IAmenityRepository Amenities => new AmenityRepository(dbContext);

	public IBookingRepository Bookings => new BookingRepository(dbContext);

	public async Task SaveAsync()
	{
		await dbContext.SaveChangesAsync();
	}
}