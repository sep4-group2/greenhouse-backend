using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _service;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(NotificationService service, ILogger<NotificationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("{greenhouseId}/past-notifications")]
    public async Task<IActionResult> GetNotificationHistoryAsync(
        [FromRoute] int greenhouseId, 
        [FromBody] NotificationQueryDTO query)
    {
        try
        {
            var notifications = await _service.GetNotificationsForPeriodAsync(greenhouseId, query.StartDate, query.EndDate);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching notifications for greenhouse {greenhouseId}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}