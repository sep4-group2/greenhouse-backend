namespace WebPushImpl.DTOs;

public class SaveSubscriptionRequestDTO
{
    public string Endpoint { get; set; }
    public string P256dh { get; set; }
    public string Auth { get; set; }
}