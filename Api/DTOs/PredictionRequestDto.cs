namespace Api.DTOs;

public class PredictionRequestDto
{
    public double Soil_Moisture { get; set; }
    public double Ambient_Temperature { get; set; }
    public double Humidity { get; set; }
}
