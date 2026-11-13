using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models;

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = "";

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = "";
}