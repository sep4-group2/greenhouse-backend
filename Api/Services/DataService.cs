using Api.DTOs;
using Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class DataService
{
    private readonly AppDbContext _ctx;

    public DataService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<List<CurrentDataResultDTO>> GetCurrentDataAsync(int greenhouseId)
    {
        var result = new List<CurrentDataResultDTO>();

        var sensorReadings = await _ctx.SensorReadings
            .Where(r => r.GreenhouseId == greenhouseId)
            .ToListAsync();

        var greenhouse = await _ctx.Greenhouses
            .Include(g => g.ActivePreset)
            .FirstOrDefaultAsync(g => g.Id == greenhouseId);

        if (greenhouse?.ActivePreset == null)
            return result;

        var types = new[] { "temperature", "air humidity", "soil humidity" };

        foreach (var type in types)
        {
            var latest = sensorReadings
                .Where(r => r.Type == type)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault();

            if (latest == null)
                continue;

            var bounds = type switch
            {
                "temperature" => (greenhouse.ActivePreset.MinTemperature, greenhouse.ActivePreset.MaxTemperature),
                "air humidity" => (greenhouse.ActivePreset.MinAirHumidity, greenhouse.ActivePreset.MaxAirHumidity),
                "soil humidity" => (greenhouse.ActivePreset.MinSoilHumidity, greenhouse.ActivePreset.MaxSoilHumidity),
                _ => (0.0, 0.0)
            };

            result.Add(new CurrentDataResultDTO
            {
                Id = latest.Id,
                Type = latest.Type,
                Value = latest.Value,
                Unit = latest.Unit,
                Timestamp = latest.Timestamp,
                MinValue = bounds.Item1,
                MaxValue = bounds.Item2
            });
        }

        return result;
    }
}
