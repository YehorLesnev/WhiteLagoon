using System.ComponentModel.DataAnnotations;

namespace WhiteLagoon.ViewModels.Auth;

public class LoginViewModel
{
	[Required]
	[DataType(DataType.EmailAddress)]
	public string Email { get; set; }

	[Required]
	[DataType(DataType.Password)]
	public string Password { get; set; }

	public bool RememberMe { get; set; }

	public string? RedirectUrl { get; set; }
}
