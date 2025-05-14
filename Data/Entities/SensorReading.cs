using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities;

public class SensorReading
{
    [Key]
    public int Id { get; set; }

    public string Type { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
    public DateTime Timestamp { get; set; }

    [Required]
    public int GreenhouseId { get; set; }

    [ForeignKey("GreenhouseId")]
    public Greenhouse Greenhouse { get; set; }
}