namespace Api.DTOs;

public class RegisterRequestDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string PasswordConfirmation { get; set; }


    public bool ConfirmPassword()
    {
        if(Password != PasswordConfirmation)
            return false;
        return true;
    }
}