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
        
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user);
        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Email = user.email
        };
        return Ok(response);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserDto userDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == userDto.Email);
        if(user == null)
            return Unauthorized("Invalid Email");
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, userDto.Password);
        if(result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid Password");
        
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user);
        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Email = user.email
        };
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var principal = getPrincipalFromExpiredToken(refreshToken, _configuration["Jwt:RefreshKey"]);
        if(principal == null)
            return Unauthorized();
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var tokenType = principal.FindFirst("token_type")?.Value;
        
        if(tokenType != "refresh")
            return Unauthorized("Invalid token type");
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        if (user == null)
            return Unauthorized("User not found");
        var newAccessToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken(user);
        
        return Ok(new
        {
            toke = newAccessToken,
            refreshToken = newRefreshToken
        });
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
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenMinutes"])),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(User user)
    {
        var refreshKey = Encoding.UTF8.GetBytes(_configuration["Jwt:RefreshKey"]);
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.email),
            new Claim("token_type", "refresh")
        };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(refreshKey), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenDays"])),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? getPrincipalFromExpiredToken(string token, string key)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateLifetime = false
        }, out SecurityToken securityToken);
        if(securityToken is not JwtSecurityToken jwt || jwt.Header.Alg != SecurityAlgorithms.HmacSha256)
            return null;
        return principal;
    }
}