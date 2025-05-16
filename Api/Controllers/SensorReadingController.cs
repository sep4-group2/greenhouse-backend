using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorReadingController : ControllerBase
{
    private readonly SensorReadingsService _sensorReadingsService;
    private readonly ILogger<SensorReadingController> _logger;

    public SensorReadingController(SensorReadingsService sensorReadingsService, ILogger<SensorReadingController> logger)
    {
        _sensorReadingsService = sensorReadingsService;
        _logger = logger;
    }
    
    [HttpGet("{greenhouseId}/past-sensor-readings/")]
    public async Task<IActionResult> GetPastSensorReadingsAsync([FromRoute] int greenhouseId, [FromQuery] PastSensorReadingRequestDTO? pastDataRequest)
    {
        try
        {
            var result =
                _sensorReadingsService.PreparePastSensorReadingsAsync(greenhouseId, pastDataRequest);
            
            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError($"Something went wrong while getting old data from the database for greenhouse {greenhouseId}: {e.Message}");
            return StatusCode(500);
        }
    }
    
    [HttpGet("{greenhouseId}/current-sensor-readings/")]
    public async Task<IActionResult> GetCurrentSensorReadingsAsync([FromRoute] int greenhouseId)
    {
        try
        {
            var result = await _sensorReadingsService.PrepareCurrentSensorReadingsAsync(greenhouseId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching sensor data for greenhouse {greenhouseId}");
            return StatusCode(500);
        }
    }
}