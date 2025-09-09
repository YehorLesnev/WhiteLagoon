using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class Repository<T>(ApplicationDbContext dbContext) : IRepository<T> where T : class
{
	internal DbSet<T> dbSet = dbContext.Set<T>();	

	public async Task AddAsync(T entity)
	{
		await dbSet.AddAsync(entity);
	}

	public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
	{
		IQueryable<T> query = dbSet;

		if (filter is not null)
		{
			query = query.Where(filter);
		}

		if (!string.IsNullOrWhiteSpace(includeProperties))
		{
			foreach(var includeProp in includeProperties.Split(','))
			{
				query = query.Include(includeProp.Trim());	
			}
		}

		return await query.AsNoTracking().ToListAsync();
	}

	public async Task<T?> GetAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
	{
		IQueryable<T> query = dbSet;

		if (filter is not null)
		{
			query = query.Where(filter);
		}

		if (!string.IsNullOrWhiteSpace(includeProperties))
		{
			foreach(var includeProp in includeProperties.Split(','))
			{
				query = query.Include(includeProp.Trim());	
			}
		}

		return await query.AsNoTracking().FirstOrDefaultAsync();
	}

	public void Remove(T entity)
	{
		dbSet.Remove(entity);
	}
}