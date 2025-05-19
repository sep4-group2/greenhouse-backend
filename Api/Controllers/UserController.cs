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
    public async Task<ActionResult<UserDto>> DeleteUser()
    {
        await _userService.DeleteUser(User.FindFirstValue(ClaimTypes.Email));
        return Ok("User deleted successfully");
    }
}