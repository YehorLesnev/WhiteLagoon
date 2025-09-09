namespace WhiteLagoon.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
	Task<IEnumerable<T>> GetAllAsync(System.Linq.Expressions.Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
	
	Task<T?> GetAsync(System.Linq.Expressions.Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
	
	Task AddAsync(T entity);

	void Remove(T entity);
}