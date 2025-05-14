using Api.DTOs;

namespace Api.Services;

public interface INotificationService
{
    public Task<VAPIDKeyDTO> GetPublicKey();
    public Task SaveSubscription(SaveSubscriptionDTO subscription);
}