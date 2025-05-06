using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Database.Entities;

public class Notification
{
    [Key]
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }
    public string Content { get; set; }

    [Required]
    public int GreenhouseId { get; set; }

    [ForeignKey("GreenhouseId")]
    public Greenhouse Greenhouse { get; set; }

    public ICollection<User> Users { get; set; }
}