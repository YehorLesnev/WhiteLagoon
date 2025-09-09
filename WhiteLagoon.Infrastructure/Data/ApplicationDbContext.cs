using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
	{
	}

	public DbSet<Villa> Villas { get; set; }

	public DbSet<VillaNumber> VillaNumbers { get; set; }

	public DbSet<Amenity> Amenity { get; set; }

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

		modelBuilder.Entity<VillaNumber>().HasData(
			new VillaNumber
			{
				Villa_Number = 101,
				VillaId = 1,
			},
			new VillaNumber
			{
				Villa_Number = 102,
				VillaId = 1,
			},
			new VillaNumber
			{
				Villa_Number = 103,
				VillaId = 1,
			},
			new VillaNumber
			{
				Villa_Number = 201,
				VillaId = 2,
			},
			new VillaNumber
			{
				Villa_Number = 202,
				VillaId = 2,
			},
			new VillaNumber
			{
				Villa_Number = 203,
				VillaId = 2,
			},
			new VillaNumber
			{
				Villa_Number = 301,
				VillaId = 3,
			},
			new VillaNumber
			{
				Villa_Number = 302,
				VillaId = 3,
			},
			new VillaNumber
			{
				Villa_Number = 303,
				VillaId = 3,
			}
		);

		modelBuilder.Entity<Amenity>().HasData(
			new Amenity
			{
				Id = 1,
				Name = "Private Pool",
				Description = "Enjoy your own private pool with stunning views.",
				VillaId = 1,
			},
			new Amenity
			{
				Id = 2,
				Name = "Free Wi-Fi",
				Description = "Stay connected with complimentary high-speed internet access.",
				VillaId = 1,
			},
			new Amenity
			{
				Id = 3,
				Name = "Ocean View",
				Description = "Wake up to breathtaking ocean views from your villa.",
				VillaId = 2,
			},
			new Amenity
			{
				Id = 4,
				Name = "Gourmet Kitchen",
				Description = "Prepare delicious meals in a fully equipped gourmet kitchen.",
				VillaId = 2,
			},
			new Amenity
			{
				Id = 5,
				Name = "Spa Bath",
				Description = "Relax and unwind in a luxurious spa bath.",
				VillaId = 3,
			},
			new Amenity
			{
				Id = 6,
				Name = "Fitness Center",
				Description = "Stay active with access to a state-of-the-art fitness center.",
				VillaId = 3,
			},
			new Amenity
			{
				Id = 7,
				Name = "24/7 Concierge",
				Description = "Experience exceptional service with our 24/7 concierge.",
				VillaId = 1,
			},
			new Amenity
			{
				Id = 8,
				Name = "Private Beach Access",
				Description = "Enjoy exclusive access to a pristine private beach.",
				VillaId = 2,
			},
			new Amenity
			{
				Id = 9,
				Name = "In-Villa Dining",
				Description = "Savor gourmet meals in the comfort of your villa with our in-villa dining service.",
				VillaId = 3,
			}
		);
	}
}