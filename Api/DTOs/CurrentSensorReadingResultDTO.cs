namespace Api.DTOs;

public class CurrentSensorReadingResultDTO
{
    public int Id { get; set; }

    public string Type { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
    public DateTime Timestamp { get; set; }
    public double MaxValue { get; set; }
    public double MinValue { get; set; }
}