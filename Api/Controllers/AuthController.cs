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
using Microsoft.AspNetCore.Authorization;


namespace Api.Controllers;
[ApiController]
[Route("[controller]")]
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

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
    {
        if (_context.Users.Any(u => u.email == registerRequestDto.Email)) 
            return BadRequest("Email already exists");
        if(!registerRequestDto.ConfirmPassword())
            return BadRequest("Passwords do not match");

        var user = new User
        {
            email = registerRequestDto.Email,
            Password = _passwordHasher.HashPassword(null, registerRequestDto.Password)
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var token = GenerateJwtToken(user);
        var response = new LoginResponseDto
        {
            Token = token,
            Email = user.email
        };
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserDto userDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == userDto.Email);
        if(user == null)
            return Unauthorized("Invalid Email");
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, userDto.Password);
        if(result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid Password");
        
        var token = GenerateJwtToken(user);
        var response = new LoginResponseDto
        {
            Token = token,
            Email = user.email
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