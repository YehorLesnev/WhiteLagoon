using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation;

public class AmenityService(IUnitOfWork unitOfWork) : IAmenityService
{
	public async Task CreateAmenityAsync(Amenity amenity)
	{
		await unitOfWork.Amenities.AddAsync(amenity);
		await unitOfWork.SaveAsync();
	}

	public async Task DeleteAmenityAsync(Amenity amenity)
	{
		unitOfWork.Amenities.Remove(amenity);
		await unitOfWork.SaveAsync();
	}

	public async Task<List<Amenity>> GetAllAmenitiesAsync()
	{
		return await unitOfWork.Amenities.GetAllAsync(includeProperties: nameof(Amenity.Villa));
	}

	public async Task<Amenity?> GetAmenityByIdAsync(int id)
	{
		return await unitOfWork.Amenities.GetAsync(x => x.Id == id, includeProperties: nameof(Amenity.Villa));
	}

	public async Task UpdateAmenityAsync(Amenity amenity)
	{
		unitOfWork.Amenities.Update(amenity);
		await unitOfWork.SaveAsync();
	}
}