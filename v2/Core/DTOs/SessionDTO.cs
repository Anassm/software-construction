namespace v2.Core.DTOs;

public class SessionStartRequest
{
    public required string LicensePlate { get; set; }
    public Guid UserId { get; set; } 
}

public class SessionStopRequest
{
    public required string LicensePlate { get; set; }
    public Guid UserId { get; set; } 
}