using System.Linq.Expressions;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Interfaces;

public interface IVillaRepository
{
	Task<IEnumerable<Villa>> GetAllAsync(Expression<Func<Villa, bool>>? filter = null, string? includeProperties = null);

	Task<Villa?> GetAsync(Expression<Func<Villa, bool>>? filter = null, string? includeProperties = null);

	Task AddAsync(Villa entity);

	void Update(Villa entity);

	void Remove(Villa entity);

	Task SaveAsync();
}