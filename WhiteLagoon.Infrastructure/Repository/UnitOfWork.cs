using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
	public IVillaRepository Villas => new VillaRepository(dbContext);
}