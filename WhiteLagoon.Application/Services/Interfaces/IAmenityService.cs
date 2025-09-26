using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces;

public interface IAmenityService
{
	Task<List<Amenity>> GetAllAmenitiesAsync();

	Task<Amenity?> GetAmenityByIdAsync(int id);

	Task CreateAmenityAsync(Amenity amenity);

	Task UpdateAmenityAsync(Amenity amenity);

	Task DeleteAmenityAsync(Amenity amenity);

}