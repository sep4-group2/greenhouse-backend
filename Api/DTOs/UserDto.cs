namespace Api.DTOs;

public class UserDto
{
    public string Email { get; set; }
    public string Password { get; set; }

    public UserDto(string email, string password)
    {
        Email = email;
        Password = password;
    }
}