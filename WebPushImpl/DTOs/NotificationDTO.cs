using System.ComponentModel.DataAnnotations;

namespace WebPushImpl.DTOs;

public class NotificationDTO
{
    public string title { get; set; }
    [Required]
    public string message { get; set; }
}