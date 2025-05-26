using System.Security.Claims;
using Api.DTOs;
using Api.Middleware;
using Api.Services;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PresetController : ControllerBase
{
    private readonly PresetService _presetService;

    public PresetController(PresetService presetService)
    {
        _presetService = presetService;
    }

    [AuthenticateUser]
    [HttpPost]
    public async Task<IActionResult> CreatePreset([FromBody] CreatePresetDTO preset)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var email = User.FindFirstValue(ClaimTypes.Email);

        var created = await _presetService.CreatePresetAsync(preset, email);
        return CreatedAtAction(nameof(GetPreset), new { id = created.Id }, created);
    }

    [AuthenticateUser]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPreset(int id)
    {
        var preset = await _presetService.GetPresetAsync(id);
        if (preset == null)
            return NotFound();

        return Ok(preset);
    }
    
    //get all presets for the user
    [AuthenticateUser]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllPresets()
    {
        // Get user's email using JWT
        string? email = User.FindFirstValue(ClaimTypes.Email);

        // If no such user exists, return unauthorized
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized("User does not exist");
        }

        var presets = await _presetService.GetAllPresetsAsync(email);
        return Ok(presets);
    }

    [AuthenticateUser]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePreset(int id, [FromBody] UpdatePresetDTO preset)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        //Get user's email using jwt 
        string email = User.FindFirstValue(ClaimTypes.Email);
        
        //If no such user exists, throw an error
        if (email == null)
        {
            throw new Exception("User does not exist");
        }

        var updated = await _presetService.UpdatePresetAsync(id, preset, email);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [AuthenticateUser]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePreset(int id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var deleted = await _presetService.DeletePresetAsync(id, email);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}