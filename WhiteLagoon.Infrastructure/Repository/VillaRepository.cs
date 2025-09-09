using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class VillaRepository(ApplicationDbContext dbContext) : IVillaRepository
{
	public async Task AddAsync(Villa entity)
	{
		await dbContext.AddAsync(entity);
	}

	public async Task<IEnumerable<Villa>> GetAllAsync(Expression<Func<Villa, bool>>? filter = null, string? includeProperties = null)
	{
		IQueryable<Villa> query = dbContext.Set<Villa>();

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

		return await query.ToListAsync();
	}

	public async Task<Villa?> GetAsync(Expression<Func<Villa, bool>>? filter = null, string? includeProperties = null)
	{
		IQueryable<Villa> query = dbContext.Set<Villa>();

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

		return await query.FirstOrDefaultAsync();
	}

	public void Update(Villa entity)
	{
		dbContext.Update(entity);
	}

	public void Remove(Villa entity)
	{
		dbContext.Remove(entity);
	}

	public async Task SaveAsync()
	{
		await dbContext.SaveChangesAsync();
	}
}
