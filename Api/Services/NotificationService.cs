using System.Text.Json;
using Api.DTOs;
using Data;
using Data.Entities;
using WebPush;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class NotificationService: INotificationService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _ctx;
    private readonly ILogger<NotificationService> _logger;
    public NotificationService(IConfiguration configuration, AppDbContext ctx, ILogger<NotificationService> logger)
    {
        _logger = logger;
        _configuration = configuration;
        _ctx = ctx;
    }
    public async Task<VAPIDKeyDTO> GetPublicKey()
    {
        //Gets public VAPID key from appsettings.json
        //Needs to be changed to cloud vault later on!
        return new VAPIDKeyDTO()
        {
            publicKey = _configuration.GetValue<string>("VAPIDKeys:PublicKey"),
        };
    }

    //Saves a subscription connected to a user in the database
    public async Task SaveSubscription(SaveSubscriptionDTO subscription)
    {
        await _ctx.Devices.AddAsync(new Device()
        {
            Auth = subscription.Auth,
            Endpoint = subscription.Endpoint,
            P256dh = subscription.P256dh,
            UserEmail = subscription.userEmail
        });
        await _ctx.SaveChangesAsync();
    }

    public async Task SendNotification(SendNotificationDTO notification)
    {
        //Get all devices connected to the user
        List<Device> devices = _ctx.Devices.Where(d => d.UserEmail == notification.userEmail).ToList();
        
        //Go through subscriptions and send the notifications
        foreach (Device device in devices)
        {
            //Create a subscription for each device connected to the user
            PushSubscription subscription = new PushSubscription(device.Endpoint, device.P256dh, device.UserEmail);
            Dictionary<string, object> options = new Dictionary<string, object>();
            options["vapidDetails"] = new VapidDetails($"mailto:{device.UserEmail}", _configuration.GetValue<string>("VAPIDKeys:PublicKey"), _configuration.GetValue<string>("VAPIDKeys:PrivateKey"));
            WebPushClient client = new WebPushClient();
 
            //Send off notification
            try
            {
                string message =
                    JsonSerializer.Serialize(notification.notification);
                await client.SendNotificationAsync(subscription, message,
                    options);
                
                //Save notification in the database
                await SaveNotification(new SaveNotificationDTO()
                {
                    Content = message,
                    GreenhouseId = notification.GreenhouseId,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        
    }
    
    //Method to save any outgoing notifications to the database, so that the client can access them, even if it doesn't reach them
    public async Task SaveNotification(SaveNotificationDTO notification)
    {
        await _ctx.Notifications.AddAsync(new Notification()
        {
            Content = notification.Content,
            GreenhouseId = notification.GreenhouseId,
            Timestamp = notification.Timestamp
        });
        await _ctx.SaveChangesAsync();
    }
    
    public async Task<List<NotificationResultDTO>> GetNotificationsForPeriodAsync(int greenhouseId, DateTime start, DateTime end)
    {
        var notifications = await _ctx.Notifications
            .Where(n => n.GreenhouseId == greenhouseId &&
                        n.Timestamp >= start &&
                        n.Timestamp <= end)
            .Select(n => new NotificationResultDTO
            {
                Id = n.Id,
                Timestamp = n.Timestamp,
                Content = n.Content
            })
            .ToListAsync();

        return notifications;
    }
}