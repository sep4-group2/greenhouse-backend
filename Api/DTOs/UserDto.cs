using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class UserDto
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }

    public UserDto(string email, string password)
    {
        Email = email;
        Password = password;
    }
}