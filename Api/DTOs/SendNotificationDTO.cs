using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class SendNotificationDTO
{
    [Required]
    public string userEmail { get; set; }
    [Required]
    public NotificationDTO notification { get; set; }
    public int GreenhouseId { get; set; }
}