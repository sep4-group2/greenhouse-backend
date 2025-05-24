using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities;

namespace Data.Entities;

public class Action
{
    [Key]
    public int Id { get; set; }

    public string Type { get; set; }
    public bool Status { get; set; }
    public DateTime Timestamp { get; set; }

    [Required]
    public int GreenhouseId { get; set; }

    [ForeignKey("GreenhouseId")]
    public Greenhouse Greenhouse { get; set; }
    
    
}