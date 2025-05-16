using Data;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PresetController : ControllerBase
{
    private readonly AppDbContext _context;

    public PresetController(AppDbContext context)
    {
        _context = context;
    }

    // Create a new preset
    [HttpPost]
    public async Task<IActionResult> CreatePreset([FromBody] Preset preset)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Presets.Add(preset);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPreset), new { id = preset.Id }, preset);
    }

    // Get a preset by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPreset(int id)
    {
        var preset = await _context.Presets.FindAsync(id);
        if (preset == null)
            return NotFound();

        return Ok(preset);
    }

    // Update an existing preset
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePreset(int id, [FromBody] Preset preset)
    {
        if (id != preset.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Entry(preset).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Presets.Any(p => p.Id == id))
                return NotFound();

            throw;
        }

        return NoContent();
    }

    // Delete a preset
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePreset(int id)
    {
        var preset = await _context.Presets.FindAsync(id);
        if (preset == null)
            return NotFound();

        _context.Presets.Remove(preset);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}