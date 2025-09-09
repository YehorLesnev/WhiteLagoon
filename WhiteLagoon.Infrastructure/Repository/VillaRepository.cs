using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class VillaRepository(ApplicationDbContext dbContext) 
	: Repository<Villa>(dbContext), IVillaRepository
{
	public void Update(Villa entity)
	{
		dbSet.Update(entity);
	}

	public async Task SaveAsync()
	{
		await dbContext.SaveChangesAsync();
	}
}