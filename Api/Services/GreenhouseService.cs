using Api.DTOs;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class GreenhouseService(AppDbContext dbContext, ConfigurationService configurationService)
{
    public async Task<List<Greenhouse>> GetAllGreenhousesForUser(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
        if(user == null)
            throw new UnauthorizedAccessException("User not found");
        return await dbContext.Greenhouses
            .Where(g => g.UserEmail == email)
            .Include(g => g.ActivePreset)
            .ToListAsync();
    }
    public async Task<Greenhouse> SetPresetAsync(Greenhouse greenhouse, Preset preset)
    {
        ArgumentNullException.ThrowIfNull(greenhouse);
        ArgumentNullException.ThrowIfNull(preset);

        dbContext.Greenhouses.Update(greenhouse);
        await dbContext.SaveChangesAsync();

        greenhouse.ActivePreset = preset;
        return greenhouse;
    }

    public async Task PairGreenhouse(GreenhousePairDto greenhousedto, string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.email == email);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");
        var greenhouse = await dbContext.Greenhouses.FirstOrDefaultAsync(g => g.MacAddress == greenhousedto.MacAddress);
        if(greenhouse == null)
        {
            greenhouse = new Greenhouse
            {
                Name = greenhousedto.Name,
                MacAddress = greenhousedto.MacAddress,
                UserEmail = email,
                LightingMethod = "manual",
                WateringMethod = "manual",
                FertilizationMethod = "manual",
            };
        }
        else if (greenhouse.UserEmail != null)
            throw new UnauthorizedAccessException("This Greenhouse has already paired");
        else
            greenhouse.UserEmail = email;

        dbContext.Greenhouses.Add(greenhouse);
        await dbContext.SaveChangesAsync();
    }

    public async Task UnpairGreenhouse(int id, string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var greenhouse = await dbContext.Greenhouses.FirstOrDefaultAsync(g => g.Id == id);
        if (greenhouse == null)
            throw new UnauthorizedAccessException("Greenhouse not found or not paired with this user");
        greenhouse.UserEmail = null;
        greenhouse.User = null;
        dbContext.Greenhouses.Update(greenhouse);
        await dbContext.SaveChangesAsync();
    }

    public async Task RenameGreenhouse(GreenhouseRenameDto greenhousedto, string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var greenhouse = await dbContext.Greenhouses.FirstOrDefaultAsync(g => g.Id == greenhousedto.Id);
        if (greenhouse == null || greenhouse.UserEmail != email)
            throw new UnauthorizedAccessException("Greenhouse not found or not paired with this user");
        greenhouse.Name = greenhousedto.Name;
        dbContext.Greenhouses.Update(greenhouse);
        await dbContext.SaveChangesAsync();
    }

    public async Task SetPresetToGreenhouse(int greenhouseId, int presetId, string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var greenhouse = await dbContext.Greenhouses.FirstOrDefaultAsync(g => g.Id == greenhouseId);
        if (greenhouse == null || greenhouse.UserEmail != email)
            throw new UnauthorizedAccessException("Greenhouse not found or not paired with this user");
        var preset = await dbContext.Presets.FirstOrDefaultAsync(p => p.Id == presetId);
        if(preset == null)
           throw new UnauthorizedAccessException("Preset not found or not paired with this user");
           
        greenhouse.ActivePresetId = preset.Id;
        dbContext.Greenhouses.Update(greenhouse);
        await dbContext.SaveChangesAsync();
        await configurationService.SendConfiguration(preset, greenhouse);
    }

    public async Task SetConfigurationForGreenhouse(string email, int greenhouseId, ConfigurationDto configuration)
    {
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var greenhouse = await dbContext.Greenhouses.FirstOrDefaultAsync(g => g.Id == greenhouseId);
        if (greenhouse == null || greenhouse.UserEmail != email)
            throw new UnauthorizedAccessException("Greenhouse not found or not paired with this user");
        switch (configuration.Type)
        {
            case "lighting":
                greenhouse.LightingMethod = configuration.Method;
                break;
            case "watering":
                greenhouse.WateringMethod = configuration.Method;
                break;
            case "fertilization":
                greenhouse.FertilizationMethod = configuration.Method;
                break;
            default:
                throw new UnauthorizedAccessException("Unknown configuration type");
        }
        var preset = await dbContext.Presets.FirstOrDefaultAsync(p => p.Id == greenhouse.ActivePresetId);
        if(preset == null)
            throw new UnauthorizedAccessException("Preset not found or not paired with this user");
        
        dbContext.Greenhouses.Update(greenhouse);
        await dbContext.SaveChangesAsync();
        await configurationService.SendConfiguration(preset, greenhouse);
    }
}