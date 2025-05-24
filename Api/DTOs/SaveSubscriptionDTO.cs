using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class SaveSubscriptionDTO
{
    [Required]
    public string userEmail { get; set; }
    [Required]
    public string Endpoint { get; set; }
    [Required]
    public string P256dh { get; set; }
    [Required]
    public string Auth { get; set; }
}