using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Extensions.WebApplicationExtensions;

public static class ApplyMigrationsExtensions
{
	public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.MigrateAsync();
        }
}