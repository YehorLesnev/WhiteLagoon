using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation;

public class VillaNumberService(IUnitOfWork unitOfWork) : IVillaNumberService
{
	public async Task CreateVillaNumberAsync(VillaNumber villa)
	{
		await unitOfWork.VillaNumbers.AddAsync(villa);
		await unitOfWork.SaveAsync();
	}

	public async Task DeleteVillaNumberAsync(VillaNumber villa)
	{
		var villaToDelete = await unitOfWork.VillaNumbers.GetAsync(v => v.Villa_Number == villa.Villa_Number);

		if (villaToDelete is null)
			return;

		unitOfWork.VillaNumbers.Remove(villaToDelete);
		await unitOfWork.SaveAsync();
	}

	public async Task<bool> ExistsAsync(int id)
	{
		return await unitOfWork.VillaNumbers.AnyAsync(v => v.Villa_Number == id);
	}

	public async Task<List<VillaNumber>> GetAllVillaNumbersAsync()
	{
		return await unitOfWork.VillaNumbers.GetAllAsync(includeProperties: nameof(VillaNumber.Villa));
	}

	public async Task<List<VillaNumber>> GetAllVillaNumbersByVillaIdAsync(int villaId)
	{
		return await unitOfWork.VillaNumbers.GetAllAsync(n => n.VillaId == villaId, includeProperties: nameof(VillaNumber.Villa));
	}

	public async Task<VillaNumber?> GetVillaNumberByIdAsync(int id)
	{
		return await unitOfWork.VillaNumbers.GetAsync(x => x.Villa_Number == id);
	}

	public async Task UpdateVillaNumberAsync(VillaNumber villa)
	{
		unitOfWork.VillaNumbers.Update(villa);
		await unitOfWork.SaveAsync();
	}
}