using Data.Database;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class DataController: ControllerBase
{
    private readonly AppDbContext _ctx;
    
    [HttpGet("{greenhouseId}")]
    public async Task<IActionResult> GetPastDataAsync([FromRoute] int greenhouseId, [FromQuery] DateTime? beforeDate = null, [FromQuery] DateTime? afterDate = null, [FromQuery] string? sensorType = null)
    {
        List<SensorReading> sensorReadings = new List<SensorReading>();
        try
        {
            //First, load all sensor readings for one specific greenhouse
            sensorReadings = _ctx.SensorReadings.Where(reading => reading.GreenhouseId == greenhouseId).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong while getting old data from the database for greenhouse {greenhouseId}: {e.Message}");
        }
        
        return Ok(sensorReadings);
    }
}