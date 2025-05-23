using Api.DTOs;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class PresetService
{
    private readonly AppDbContext _ctx;
    private readonly ConfigurationService _configuration;

    public PresetService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Preset?> GetPresetAsync(int id)
    {
        return await _ctx.Presets.FindAsync(id);
    }

    public async Task<Preset> CreatePresetAsync(CreatePresetDTO preset)
    {
        //Check if user preset is null, if it is, throw an error
        if (preset.UserEmail == null)
        {
            throw new Exception("User preset cannot be null");
        }
        
        var createdPreset = _ctx.Presets.Add(new Preset()
        {
            HoursOfLight = preset.HoursOfLight,
            MaxAirHumidity = preset.MaxAirHumidity,
            MaxSoilHumidity = preset.MaxSoilHumidity,
            MaxTemperature = preset.MaxTemperature,
            MinAirHumidity = preset.MinAirHumidity,
            MinSoilHumidity = preset.MinSoilHumidity,
            MinTemperature = preset.MinTemperature,
            Name = preset.Name,
        });
        
        //If not null, save the user preset
        _ctx.UserPresets.Add(new UserPreset()
        {
            UserEmail = preset.UserEmail,
            Preset = createdPreset.Entity
        });
        
        await _ctx.SaveChangesAsync();
        
        Preset result = await _ctx.Presets.FindAsync(createdPreset.Entity.Id);
        
        return result;
    }

    public async Task<bool> UpdatePresetAsync(int id, UpdatePresetDTO preset, string userEmail)
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
        
        //Get greenhouses connected to this preset
        var greenhouses = await _ctx.Greenhouses.Where(g => g.ActivePresetId == savedPreset.Id ).ToListAsync();
        
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
        
        //Modify preset to save
        savedPreset.Name = preset.Name;
        savedPreset.MinTemperature = preset.MinTemperature;
        savedPreset.MaxTemperature = preset.MaxTemperature;
        savedPreset.MinAirHumidity = preset.MinAirHumidity;
        savedPreset.MaxAirHumidity = preset.MaxAirHumidity;
        savedPreset.MinSoilHumidity = preset.MinSoilHumidity;
        savedPreset.MaxSoilHumidity = preset.MaxSoilHumidity;
        savedPreset.HoursOfLight = preset.HoursOfLight;

        try
        {
            _ctx.Presets.Update(savedPreset);
            await _ctx.SaveChangesAsync();
            
            //After the preset has been updated in the database, go through all greenhouses connected to it, and update them too
            greenhouses.ForEach(async g =>
            {
                await _configuration.SendConfiguration(savedPreset, g);
            });
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
        
        //Check if the preset is connected to any greenhouses
        var connectedGreenhouses = await _ctx.Greenhouses.Where(g => g.ActivePresetId == id).ToListAsync();

        //If it is it cannot be deleted
        if (connectedGreenhouses != null || connectedGreenhouses.Count > 0)
        {
            throw new Exception("Cannot delete preset currently in use");
        }
        
        if (preset == null)
            return false;

        _ctx.Presets.Remove(preset);
        await _ctx.SaveChangesAsync();
        return true;
    }
}