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

        var created = await _presetService.CreatePresetAsync(preset);
        return CreatedAtAction(nameof(GetPreset), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPreset(int id)
    {
        var preset = await _presetService.GetPresetAsync(id);
        if (preset == null)
            return NotFound();

        return Ok(preset);
    }

    [AuthenticateUser]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePreset(int id, [FromBody] Preset preset)
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePreset(int id)
    {
        var deleted = await _presetService.DeletePresetAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}