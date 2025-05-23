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
    
    [HttpGet()]
    [AuthenticateUser]
    public async Task<ActionResult<List<GreenhouseDto>>> GetAllGreenhouses()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        
        var greenhouses = await _greenhouseService.GetAllGreenhousesForUser(email!);
        
        // map it to DTOs
        return greenhouses.Select(g => new GreenhouseDto
        {
            Id = g.Id,
            Name = g.Name,
            MacAddress = g.MacAddress,
            LightingMethod = g.LightingMethod,
            WateringMethod = g.WateringMethod,
            FertilizationMethod = g.FertilizationMethod,
            UserEmail = g.UserEmail,
            ActivePresetId = g.ActivePresetId,
            ActivePresetName = g.ActivePreset?.Name
        }).ToList();
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

    [HttpPut("preset/{id}")]
    [AuthenticateUser]
    public async Task<ActionResult> SetPresetToGreenhouse([FromRoute] int greenhouseId, [FromBody] int presetId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        await _greenhouseService.SetPresetToGreenhouse(greenhouseId, presetId, email);
        return Ok($"Preset with id {presetId} has been set to greenhouse {greenhouseId}");
    }

    [HttpPut("configure/{id}")]
    [AuthenticateUser]
    public async Task<ActionResult> SetConfigurationForGreenhouse([FromRoute] int greenhouseId,
        [FromBody] ConfigurationDto configuration)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        await _greenhouseService.SetConfigurationForGreenhouse(email, greenhouseId, configuration);
        return Ok($"{configuration.Type} has been set to {configuration.Type}");
    }

    [HttpPost("predict/{greenhouseId}")]
    [AuthenticateUser]
    public async Task<ActionResult<PredictionResultDto>> GetPrediction(int greenhouseId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized("Email claim missing");
        }

        var result = await _greenhouseService.GetPredictionFromLatestValuesAsync(greenhouseId);
        return Ok(result);
    }
}