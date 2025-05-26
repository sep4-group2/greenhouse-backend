using WebPushImpl.DTOs;

namespace WebPushImpl.Services;

public interface INotificationService
{
    public Task<VAPIDKeyDTO> GetPublicKey();
    public Task SaveSubscription(SaveSubscriptionDTO subscription);
    public Task SendNotification(SendNotificationDTO notification);
    public Task SaveNotification(SaveNotificationDTO notification);

    public Task<List<NotificationResultDTO>> GetNotificationsForPeriodAsync(
        int greenhouseId, DateTime start, DateTime end);
}