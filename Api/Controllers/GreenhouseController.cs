using System.Security.Claims;
using Api.DTOs;
using Api.Middleware;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GreenhouseController : ControllerBase
{
    private readonly GreenhouseService _greenhouseService;

    public GreenhouseController(GreenhouseService greenhouseService)
    {
        _greenhouseService = greenhouseService;
    }
    
    [HttpPost("pair")]
    [AuthenticateUser]
    public async Task<ActionResult> PairGreenhouse([FromBody] GreenhousePairDto greenhousePair)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        await _greenhouseService.PairGreenhouse(greenhousePair, email);
        return Ok($"{greenhousePair.Name} with MAC address: {greenhousePair.MacAddress} has been paired with {email}");
    }

    [HttpPost("unpair/{id}")]
    [AuthenticateUser]
    public async Task<ActionResult> UnpairGreenhouse([FromBody]int id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        await _greenhouseService.UnpairGreenhouse(id, email);
        return Ok($"Greenhouse with Id: {id} has been unpaired from {email}");
    }

    [HttpPut("rename/{id}")]
    [AuthenticateUser]
    public async Task<ActionResult> RenameGreenhouse([FromBody] GreenhouseRenameDto greenhouse)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        await _greenhouseService.RenameGreenhouse(greenhouse, email);
        return Ok($"Greenhouse has been renamed to {greenhouse.Name}");
    }
}