namespace DataConsumer.DTOs;

public class ActionMessageDTO
{
    public string MacAddress { get; set; }
    public string Command { get; set; }
    public bool Status { get; set; }
}