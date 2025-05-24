using Data;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task DeleteUser(string email, string password)
    {
        if(string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
        if(result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid Password");
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
    
}