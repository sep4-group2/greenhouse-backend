using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class SaveNotificationDTO
{
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }

    [Required]
    public int GreenhouseId { get; set; }
}