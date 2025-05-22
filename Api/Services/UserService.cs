using Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task DeleteUser(string email)
    {
        if(string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
    
}