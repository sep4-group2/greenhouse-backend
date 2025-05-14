using Api.DTOs;
using Api.Services;
using Data.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}