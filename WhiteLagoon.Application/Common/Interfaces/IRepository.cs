using System.Linq.Expressions;

namespace WhiteLagoon.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
	Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
	
	Task<T?> GetAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
	
	Task AddAsync(T entity);

	void Remove(T entity);

	Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null);
}