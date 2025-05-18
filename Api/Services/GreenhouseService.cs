using Api.DTOs;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class GreenhouseService(AppDbContext dbContext)
{
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
        bool alreadyPaired = await dbContext.Greenhouses.AnyAsync(g => g.IpAddress == greenhousedto.MacAddress && g.UserEmail != null);
        if (alreadyPaired)
            throw new UnauthorizedAccessException("This Greenhouse has already paired");
        
        var user = await dbContext.Users.Include(u => u.Greenhouses).FirstOrDefaultAsync(u => u.email == email);
        if(user == null)
            throw new UnauthorizedAccessException("User not found");
        Greenhouse greenhouse = new Greenhouse
        {
            Name = greenhousedto.Name,
            IpAddress = greenhousedto.MacAddress,
            UserEmail = email,
            LightingMethod = "Automatic",
            WateringMethod = "Automatic",
            FertilizationMethod = "Automatic",
        };
        dbContext.Greenhouses.Add(greenhouse);
        await dbContext.SaveChangesAsync();
    }

    public async Task unpairGreenhouse(int id, string email)
    {
        if(string.IsNullOrEmpty(email))
            throw new UnauthorizedAccessException("Email claim missing");
        var greenhouse = await dbContext.Greenhouses.FirstOrDefaultAsync(g => g.Id == id);
        if(greenhouse == null)
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
        if(greenhouse == null || greenhouse.UserEmail != email)
            throw new UnauthorizedAccessException("Greenhouse not found or not paired with this user");
        greenhouse.Name = greenhousedto.Name;
        dbContext.Greenhouses.Update(greenhouse);
        await dbContext.SaveChangesAsync();
    }

}