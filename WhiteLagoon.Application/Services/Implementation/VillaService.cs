using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Application.Utility.Constants;
using WhiteLagoon.Application.Utility.Helpers;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation;

public class VillaService(IUnitOfWork unitOfWork) : IVillaService
{
	public async Task CreateVillaAsync(Villa villa, string basePath)
	{
		if (villa.Image is not null)
		{
			string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
			string imagePath = Path.Combine(basePath, @"images\VillaImage", fileName);

			using var fileStream = new FileStream(imagePath, FileMode.Create);
			await villa.Image.CopyToAsync(fileStream);

			villa.ImageUrl = $@"\images\VillaImage\{fileName}";
		}
		else
		{
			villa.ImageUrl = "https://placehold.co/600x400";
		}

		await unitOfWork.Villas.AddAsync(villa);
		await unitOfWork.SaveAsync();
	}

	public async Task DeleteVillaAsync(Villa villa, string basePath)
	{
		var villaToDelete = await unitOfWork.Villas.GetAsync(v => v.Id == villa.Id);

		if (villaToDelete is not null)
		{
			unitOfWork.Villas.Remove(villaToDelete);
			await unitOfWork.SaveAsync();

			if (!string.IsNullOrEmpty(villaToDelete.ImageUrl))
			{
				string imagePath = Path.Combine(basePath, villaToDelete.ImageUrl.TrimStart('\\'));

				if (System.IO.File.Exists(imagePath))
				{
					System.IO.File.Delete(imagePath);
				}
			}

		}
	}

	public async Task<List<Villa>> GetAllVillasAsync()
	{
		return await unitOfWork.Villas.GetAllAsync(includeProperties: nameof(Villa.VillaAmenities));
	}

	public async Task<Villa?> GetVillaByIdAsync(int id)
	{
		return await unitOfWork.Villas.GetAsync(x => x.Id == id, includeProperties: nameof(Villa.VillaAmenities));
	}

	public async Task<List<Villa>> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate)
	{
		var villaList = await unitOfWork.Villas.GetAllAsync(includeProperties: nameof(Villa.VillaAmenities));
		var villaNumbers = await unitOfWork.VillaNumbers.GetAllAsync();
		var bookings = await unitOfWork.Bookings.GetAllAsync(b =>
			b.Status == BookingStatusConstants.Approved || b.Status == BookingStatusConstants.CheckedIn);

		foreach (var villa in villaList ?? [])
		{
			int roomsAvailable = VillaRoomsAvailabilityHelper.GetNumberOfAvailableRooms(villa.Id, villaNumbers, checkInDate, nights, bookings);

			villa.IsAvailable = roomsAvailable > 0;
		}	
		
		return villaList ?? [];
	}

	public async Task UpdateVillaAsync(Villa villa, string basePath)
	{
		if (villa.Image is not null)
		{
			if (!string.IsNullOrEmpty(villa.ImageUrl))
			{
				string oldImagePath = Path.Combine(basePath, villa.ImageUrl.TrimStart('\\'));

				if (System.IO.File.Exists(oldImagePath))
				{
					System.IO.File.Delete(oldImagePath);
				}
			}

			string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
			string imagePath = Path.Combine(basePath, @"images\VillaImage", fileName);

			using var fileStream = new FileStream(imagePath, FileMode.Create);
			await villa.Image.CopyToAsync(fileStream);

			villa.ImageUrl = $@"\images\VillaImage\{fileName}";
		}

		unitOfWork.Villas.Update(villa);
		await unitOfWork.SaveAsync();
	}
}