using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Api.Services;

namespace Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
    {
        var user = await _authService.RegisterUser(registerRequestDto);
        var accessToken = _authService.GenerateJwtToken(user);
        var refreshToken = _authService.GenerateRefreshToken(user);
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
        var user = await _authService.Login(userDto);
        var accessToken = _authService.GenerateJwtToken(user);
        var refreshToken = _authService.GenerateRefreshToken(user);
        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Email = user.email
        };
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var user = await _authService.RefreshToken(refreshToken);
        var newAccessToken = _authService.GenerateJwtToken(user);
        var newRefreshToken = _authService.GenerateRefreshToken(user);
        
        return Ok(new
        {
            token = newAccessToken,
            refreshToken = newRefreshToken
        });
    }
}
