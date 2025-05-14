namespace Api.DTOs;

public class PastDataResultDTO
{
    public int Id { get; set; }

    public string Type { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
    public DateTime Timestamp { get; set; }
}