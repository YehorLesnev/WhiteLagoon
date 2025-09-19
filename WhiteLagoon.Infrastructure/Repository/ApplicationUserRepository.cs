using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class ApplicationUserRepository(ApplicationDbContext dbContext)
	: Repository<ApplicationUser>(dbContext), IApplicationUserRepository
{
	public void Update(ApplicationUser entity)
	{
		dbSet.Update(entity);
	}
}