using System.Security.Claims;
using System.Text;
using Api.DTOs;
using Data.Database;
using Data.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;


namespace Api.Controllers;

public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string email, string password, string confirmPassword)
    {
        if (_context.Users.Any(u => u.email == email)) 
            return BadRequest("Email already exists");
        if(password != confirmPassword)
            return BadRequest("Passwords do not match");

        var user = new User
        {
            email = email,
            Password = "password"
        };
        user.Password = _passwordHasher.HashPassword(user, password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var token = GenerateJwtToken(user);
        var response = new LoginResponseDto
        {
            Token = token,
            User = new UserDto(email, password)
        };
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        if(user == null)
            return Unauthorized("Invalid Email");
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
        if(result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid Password");
        
        var token = GenerateJwtToken(user);
        var response = new LoginResponseDto
        {
            Token = token,
            User = new UserDto(email, password)
        };
        return Ok(response);
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.email),
            new Claim(ClaimTypes.Name, user.email)
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:Expiration"])),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
}