using Api.DTOs;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class PresetService
{
    private readonly AppDbContext _ctx;
    private readonly ConfigurationService _configuration;

    public PresetService(AppDbContext ctx, ConfigurationService configuration)
    {
        _ctx = ctx;
        _configuration = configuration;
    }

    public async Task<Preset?> GetPresetAsync(int id)
    {
        return await _ctx.Presets.FindAsync(id);
    }

    public async Task<Preset> CreatePresetAsync(CreatePresetDTO preset, string userEmail)
    {
        // Check if user preset is null, if it is, throw an error
        if (userEmail == null)
        {
            throw new Exception("User preset cannot be null");
        }

        var createdPreset = _ctx.Presets.Add(new Preset
        {
            HoursOfLight = preset.HoursOfLight,
            MaxAirHumidity = preset.MaxAirHumidity,
            MaxSoilHumidity = preset.MaxSoilHumidity,
            MaxTemperature = preset.MaxTemperature,
            MinAirHumidity = preset.MinAirHumidity,
            MinSoilHumidity = preset.MinSoilHumidity,
            MinTemperature = preset.MinTemperature,
            Name = preset.Name
        });
        
        await _ctx.SaveChangesAsync();
        
        // If not null, save the user preset
        _ctx.UserPresets.Add(new UserPreset
        {
            UserEmail = userEmail,
            Preset = createdPreset.Entity
        });

        await _ctx.SaveChangesAsync();

        var result = await _ctx.Presets.FindAsync(createdPreset.Entity.Id);

        if (result == null)
        {
            throw new Exception("Failed to create preset");
        }

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

    public async Task<bool> DeletePresetAsync(int id, string userEmail)
    {
        var preset = await _ctx.Presets
            .Include(p => p.UserPreset)
            .Include(p => p.SystemPreset)
            .FirstOrDefaultAsync(p => p.Id == id);
    
        if (preset == null)
            return false;
    
        // Check if it's a system preset
        if (preset.SystemPreset != null)
        {
            throw new Exception("Cannot delete system preset");
        }
    
        // Check if preset belongs to requesting user
        if (preset.UserPreset?.UserEmail != userEmail)
        {
            throw new Exception("Cannot delete preset belonging to other users");
        }
    
        // Check if connected to greenhouses
        var isConnected = await _ctx.Greenhouses.AnyAsync(g => g.ActivePresetId == id);
        if (isConnected)
        {
            throw new Exception("Cannot delete preset currently in use");
        }
    
        _ctx.Presets.Remove(preset);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Preset>> GetAllPresetsAsync(string userEmail)
    {
        // Fetch system presets
        var systemPresets = await _ctx.Presets
            .Include(p => p.SystemPreset)
            .Where(p => p.SystemPreset != null)
            .ToListAsync();

        // Fetch user-specific presets
        var userPresets = await _ctx.Presets
            .Include(p => p.UserPreset)
            .Where(p => p.UserPreset != null && p.UserPreset.UserEmail == userEmail)
            .ToListAsync();

        // Combine both lists
        return systemPresets.Concat(userPresets);
    }
}