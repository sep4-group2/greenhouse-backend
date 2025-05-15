using Api.DTOs;
using Data.Database;
using Data.Database.Utils;
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

    public async Task<List<PastDataResultDTO>> GetPastDataAsync(int greenhouseId, PastDataRequestDTO? pastDataRequest)
    {
        List<PastDataResultDTO> result = new List<PastDataResultDTO>();
            
        //First, load all sensor readings for one specific greenhouse
        var sensorReadings = _ctx.SensorReadings.Where(reading => reading.GreenhouseId == greenhouseId);
            
        //Filtering
        if (pastDataRequest != null)
        {
            //Start with beforeDate: sensor readings before that specific date, INCLUDING the date itself
            if (pastDataRequest.BeforeDate != null)
            {
                sensorReadings = sensorReadings.Where(reading =>
                    reading.Timestamp.CompareTo(pastDataRequest.BeforeDate) <= 0);
            }

            //afterDate: sensor readings after that specific date, INCLUDING the date itself
            if (pastDataRequest.AfterDate != null)
            {
                sensorReadings = sensorReadings.Where(reading =>
                    reading.Timestamp.CompareTo(pastDataRequest.AfterDate) >= 0);
            }
            
            //sensorType: the type of the sensor reading
            if (pastDataRequest.ReadingType != null)
            {
                sensorReadings = sensorReadings.Where(reading => reading.Type == pastDataRequest.ReadingType);
            }
            result = sensorReadings.Select(reading => new PastDataResultDTO
            {
                Id = reading.Id,
                Timestamp = reading.Timestamp,
                Type = reading.Type,
                Unit = reading.Unit,
                Value = reading.Value
            }).ToList();
        }

        return result;
    }
    
}