using Data.Database;
using Data.Database.Entities;

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
}