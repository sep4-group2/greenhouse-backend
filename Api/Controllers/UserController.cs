using System.Security.Claims;
using Api.DTOs;
using Api.Middleware;
using Api.Services;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpDelete]
    [AuthenticateUser]
    public async Task<ActionResult<UserDto>> DeleteUser(string password)
    {
        await _userService.DeleteUser(User.FindFirstValue(ClaimTypes.Email), password);
        return Ok("User deleted successfully");
    }
}