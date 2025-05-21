using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class PresetService
{
    private readonly AppDbContext _ctx;

    public PresetService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Preset?> GetPresetAsync(int id)
    {
        return await _ctx.Presets.FindAsync(id);
    }

    public async Task<Preset> CreatePresetAsync(Preset preset)
    {
        //Check if user preset is null, if it is, throw an error
        if (preset.UserPreset == null)
        {
            throw new Exception("User preset cannot be null");
        }
        
        //If not null, save the user preset
        _ctx.UserPresets.Add(preset.UserPreset);
        
        _ctx.Presets.Add(preset);
        await _ctx.SaveChangesAsync();
        return preset;
    }

    public async Task<bool> UpdatePresetAsync(int id, Preset preset, string userEmail)
    {
        if (id != preset.Id)
            return false;
        
        //Get preset from the database
        var savedPreset = await _ctx.Presets.Where(p => p.Id == id).Include(p => p.UserPreset).Include(p => p.SystemPreset).FirstOrDefaultAsync();
        
        //Check if preset returned is null or not
        if (savedPreset == null)
        {
            throw new Exception("Preset not found");
        }
        
        //Check if it is a system preset
        if (savedPreset.SystemPreset != null)
        {
            throw new Exception("Cannot modify system preset");
        }
        
        //Check if the preset belongs to the user
        if (savedPreset.UserPreset.UserEmail != userEmail)
        {
            throw new Exception("Cannot modify preset belonging to other users");
        }

        _ctx.Entry(preset).State = EntityState.Modified;

        try
        {
            await _ctx.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _ctx.Presets.AnyAsync(p => p.Id == id))
                return false;
            throw;
        }
    }

    public async Task<bool> DeletePresetAsync(int id)
    {
        var preset = await _ctx.Presets.FindAsync(id);
        if (preset == null)
            return false;

        _ctx.Presets.Remove(preset);
        await _ctx.SaveChangesAsync();
        return true;
    }
}