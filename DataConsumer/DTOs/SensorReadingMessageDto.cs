using System;
using System.Collections.Generic;

namespace DataConsumer.DTOs;

public class SensorReadingMessageDto
{
    public string MacAddress { get; set; }
    public List<SensorDataDto> SensorData { get; set; }
    // public DateTime Timestamp { get; set; }
}

public class SensorDataDto
{
    public string Type { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
}