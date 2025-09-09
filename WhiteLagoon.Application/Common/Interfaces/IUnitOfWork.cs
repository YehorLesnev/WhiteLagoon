namespace WhiteLagoon.Application.Common.Interfaces;

public interface IUnitOfWork
{
	IVillaRepository Villas { get; }
}