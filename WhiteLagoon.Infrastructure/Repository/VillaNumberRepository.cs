using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class VillaNumberRepository(ApplicationDbContext dbContext) 
	: Repository<VillaNumber>(dbContext), IVillaNumberRepository
{
	public void Update(VillaNumber entity)
	{
		dbSet.Update(entity);
	}
}