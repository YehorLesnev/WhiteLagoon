using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces;

public interface IVillaService
{
	Task<List<Villa>> GetAllVillasAsync();

	Task<Villa?> GetVillaByIdAsync(int id);

	Task CreateVillaAsync(Villa villa, string basePath);

	Task UpdateVillaAsync(Villa villa, string basePath);

	Task DeleteVillaAsync(Villa villa, string basePath);
}