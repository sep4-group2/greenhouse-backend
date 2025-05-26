using System.ComponentModel.DataAnnotations;

namespace WebPushImpl.DTOs;

public class SendNotificationDTO
{
    [Required]
    public string userEmail { get; set; }
    [Required]
    public NotificationDTO notification { get; set; }
    public int GreenhouseId { get; set; }
}