using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces;

public interface IVillaNumberService
{
	Task<List<VillaNumber>> GetAllVillaNumbersAsync();

	Task<VillaNumber?> GetVillaNumberByIdAsync(int id);

	Task CreateVillaNumberAsync(VillaNumber villa);

	Task UpdateVillaNumberAsync(VillaNumber villa);

	Task DeleteVillaNumberAsync(VillaNumber villa);
}