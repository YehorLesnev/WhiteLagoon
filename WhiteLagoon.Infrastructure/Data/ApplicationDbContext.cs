using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
	{
	}

	public DbSet<Villa> Villas { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		var dateTimeNow = DateTime.Now;

		modelBuilder.Entity<Villa>().HasData(
			new Villa
			{
				Id = 1,
				Name = "Royal Villa",
				Sqft = 550,
				Occupancy = 4,
				Price = 200,
				ImageUrl = "https://placehold.co/600x400",
				Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. ",
			},
			new Villa
			{
				Id = 2,
				Name = "Premium Pool Villa",
				Sqft = 350,
				Occupancy = 4,
				Price = 300,
				ImageUrl = "https://placehold.co/600x401",
				Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. ",
			},
			new Villa
			{
				Id = 3,
				Name = "Luxury Pool Villa",
				Sqft = 650,
				Occupancy = 4,
				Price = 400,
				ImageUrl = "https://placehold.co/600x402",
				Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. ",
			}
		);
	}
}