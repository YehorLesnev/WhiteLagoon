using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class AmenityRepository(ApplicationDbContext dbContext)
	: Repository<Amenity>(dbContext), IAmenityRepository
{
	public void Update(Amenity entity)
	{
		dbSet.Update(entity);
	}
}