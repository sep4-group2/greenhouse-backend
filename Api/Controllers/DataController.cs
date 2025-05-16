using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly DataService _dataService;
    private readonly ILogger<DataController> _logger;

    public DataController(DataService dataService, ILogger<DataController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }
    
    [HttpGet("{greenhouseId}")]
    public async Task<IActionResult> GetPastDataAsync([FromRoute] int greenhouseId, [FromQuery] PastDataRequestDTO? pastDataRequest)
    {
        try
        {
            var result =
                _dataService.GetPastDataAsync(greenhouseId, pastDataRequest);
            
            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError($"Something went wrong while getting old data from the database for greenhouse {greenhouseId}: {e.Message}");
            return StatusCode(500);
        }
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