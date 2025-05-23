using Api.DTOs;
using Data;
using Data.Utils;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class SensorReadingsService
{
    private readonly AppDbContext _ctx;

    public SensorReadingsService(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<List<CurrentSensorReadingResultDTO>> PrepareCurrentSensorReadingsAsync(int greenhouseId)
    {
        var result = new List<CurrentSensorReadingResultDTO>();

        var sensorReadings = await _ctx.SensorReadings
            .Where(r => r.GreenhouseId == greenhouseId)
            .ToListAsync();

        var greenhouse = await _ctx.Greenhouses
            .Include(g => g.ActivePreset)
            .FirstOrDefaultAsync(g => g.Id == greenhouseId);

        if (greenhouse?.ActivePreset == null)
            return result;

        var types = new[] { SensorReadingType.Temperature, SensorReadingType.AirHumidity, SensorReadingType.SoilHumidity };

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
                SensorReadingType.Temperature => (greenhouse.ActivePreset.MinTemperature, greenhouse.ActivePreset.MaxTemperature),
                SensorReadingType.AirHumidity => (greenhouse.ActivePreset.MinAirHumidity, greenhouse.ActivePreset.MaxAirHumidity),
                SensorReadingType.SoilHumidity => (greenhouse.ActivePreset.MinSoilHumidity, greenhouse.ActivePreset.MaxSoilHumidity),
                _ => (0.0, 0.0)
            };

            result.Add(new CurrentSensorReadingResultDTO
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

    public async Task<List<PastSensorReadingResultDTO>> PreparePastSensorReadingsAsync(int greenhouseId, PastSensorReadingRequestDTO? pastDataRequest)
    {
        var sensorReadings = _ctx.SensorReadings
            .Where(reading => reading.GreenhouseId == greenhouseId);

        // Filtering
        if (pastDataRequest != null)
        {
            if (pastDataRequest.BeforeDate != null)
            {
                sensorReadings = sensorReadings
                    .Where(reading => reading.Timestamp <= pastDataRequest.BeforeDate);
            }
            if (pastDataRequest.AfterDate != null)
            {
                sensorReadings = sensorReadings
                    .Where(reading => reading.Timestamp >= pastDataRequest.AfterDate);
            }
            if (pastDataRequest.ReadingType != null)
            {
                sensorReadings = sensorReadings
                    .Where(reading => reading.Type == pastDataRequest.ReadingType);
            }
        }

        var result = await sensorReadings
            .Select(reading => new PastSensorReadingResultDTO
            {
                Id = reading.Id,
                Timestamp = reading.Timestamp,
                Type = reading.Type,
                Unit = reading.Unit,
                Value = reading.Value
            })
            .ToListAsync();

        return result;
    }
}