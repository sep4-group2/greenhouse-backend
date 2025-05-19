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
        _ctx.Presets.Add(preset);
        await _ctx.SaveChangesAsync();
        return preset;
    }

    public async Task<bool> UpdatePresetAsync(int id, Preset preset)
    {
        if (id != preset.Id)
            return false;

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