using Api.DTOs;

namespace Api.Services;

public class NotificationService: INotificationService
{
    public Task<VAPIDKeyDTO> GetPublicKey()
    {
        //This needs to be changed as well
        //We need to store and get the VAPID keys from somewhere, they cannot just float out in the open unfortunately
        return Task.FromResult(new VAPIDKeyDTO());
    }

    public Task SaveSubscription(SaveSubscriptionDTO subscription)
    {
        //Needs to be saved somewhere
        //Discussion about where exactly
        throw new NotImplementedException();
    }
}