using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string PasswordConfirmation { get; set; }


    public bool ConfirmPassword()
    {
        return Password != PasswordConfirmation;
    }
}