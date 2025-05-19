using System.Security.Claims;
using Api.DTOs;
using Api.Middleware;
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
    
    //Endpoint for clients to fetch the public VAPID key needed to subscribe
    [AuthenticateUser]
    [HttpGet("/public-key")]
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
    
    
    //Endpoint for saving a subscription
    [AuthenticateUser]
    [HttpPost("save-subscription")]
    public async Task<IActionResult> SaveSubscription([FromBody] SaveSubscriptionRequestDTO subscription)
    {
        try
        {
            //Using jwt first get the user email
            string email = User.FindFirstValue(ClaimTypes.Email);

            //Check if the user has an email
            if (email == null)
            {
                return Unauthorized();
            }

            //If they do, create the subscription and save it
            await _notificationService.SaveSubscription(
                new SaveSubscriptionDTO()
                {
                    Auth = subscription.Auth,
                    Endpoint = subscription.Endpoint,
                    P256dh = subscription.P256dh,
                    userEmail = email
                });
            return Created();
        }
        catch (Exception e)
        {
            _logger.LogError($"Something went wrong while trying to save the subscription: {e.Message}");
            return StatusCode(500);
        }
    }
}