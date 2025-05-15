using Api.DTOs;

namespace Api.Services;

public class NotificationService: INotificationService
{
    private readonly IConfiguration _configuration;
    public NotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public Task<VAPIDKeyDTO> GetPublicKey()
    {
        //Gets public VAPID key from appsettings.json
        //Needs to be changed to cloud vault later on!
        return Task.FromResult(new VAPIDKeyDTO()
        {
            publicKey = _configuration.GetValue<string>("VAPIDKeys:PublicKey"),
        });
    }

    public Task SaveSubscription(SaveSubscriptionDTO subscription)
    {
        //Needs to be saved somewhere
        //Discussion about where exactly
        throw new NotImplementedException();
    }
}