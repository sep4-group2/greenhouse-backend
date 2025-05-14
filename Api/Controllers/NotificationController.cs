using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController: ControllerBase
{
    INotificationService _notificationService;
    ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }
    //Two endpoints needed 
    //One for fetching the public VAPID key
    //One for saving subscriptions

    
    //Endpoint for clients to fetch the public VAPID key needed to subscribe
    [HttpGet]
    public async Task<IActionResult> GetPublicKey()
    {
        try
        {
            var publicKey = await _notificationService.GetPublicKey();
            return Ok(publicKey);
        }
        catch (Exception e)
        {
            _logger.LogError($"Something went wrong while fetching the public VAPID key: {e.Message}" );
            return StatusCode(500);
        }
    }
}