using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Data.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteUser([FromBody] string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        if (user == null)
        {
            return NotFound("User not found");
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok("User deleted successfully");
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if(string.IsNullOrEmpty(email))
            return Unauthorized("Email claim missing");
        await DeleteUser(email);
        return Ok("User deleted successfully");
    }
}