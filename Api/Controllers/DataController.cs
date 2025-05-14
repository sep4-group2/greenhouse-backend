using Api.DTOs;
using Api.Services;
using Data;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly DataService _dataService;
    private readonly ILogger<DataController> _logger;

    public DataController(DataService dataService, ILogger<DataController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [HttpGet("{greenhouseId}")]
    public async Task<IActionResult> GetPastDataAsync([FromRoute] int greenhouseId,
        [FromQuery] PastDataRequestDTO? pastDataRequest)
    {
        List<PastDataResultDTO> result = new List<PastDataResultDTO>();
        try
        {
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
        }
        catch (Exception e)
        {
            _logger.LogError(
                $"Something went wrong while getting old data from the database for greenhouse {greenhouseId}: {e.Message}");
            return StatusCode(500);
        }

        return Ok(result);
    }

    [HttpGet("current-sensor-readings/{greenhouseId}")]
    public async Task<IActionResult> GetCurrentDataAsync([FromRoute] int greenhouseId)
    {
        try
        {
            var result = await _dataService.GetCurrentDataAsync(greenhouseId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching sensor data for greenhouse {greenhouseId}");
            return StatusCode(500);
        }
    }
}