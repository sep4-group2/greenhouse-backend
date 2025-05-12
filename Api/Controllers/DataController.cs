using Data.Database;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class DataController: ControllerBase
{
    private readonly AppDbContext _ctx;
    
    [HttpGet("{greenhouseId}")]
    public async Task<ICollection<SensorReading>> GetPastData([FromRoute] int greenhouseId)
    {
        
        return null;
    }
}