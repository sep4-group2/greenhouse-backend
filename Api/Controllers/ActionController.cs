using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActionController : ControllerBase
{
    private readonly ActionService _service;
    private readonly ILogger<ActionController> _logger;

    public ActionController(ActionService service, ILogger<ActionController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("{greenhouseId}/past-actions")]
    public async Task<IActionResult> GetActionHistoryAsync(
        [FromRoute] int greenhouseId, 
        [FromBody] ActionQueryDTO query)
    {
        try
        {
            var actions = await _service.PrepareActionsForPeriodAsync(greenhouseId, query.StartDate, query.EndDate);
            return Ok(actions);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching actions for greenhouse {greenhouseId}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpPost("{greenhouseId}/actions")]
    public async Task<IActionResult> SaveActionAsync(
        [FromRoute] int greenhouseId,
        [FromBody] ActionCreateDTO actionDto)
    {
        try
        {
            var action = new Data.Entities.Action
            {
                Type = actionDto.Type,
                Status = actionDto.Status,
                Timestamp = DateTime.UtcNow,
                GreenhouseId = greenhouseId
            };

            await _service.SaveActionAsync(action);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving action for greenhouse {greenhouseId}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}