namespace v2.Core.Interfaces;

using v2.Core.DTOs;

public interface IParkingLots
{
    Task<(int statusCode, object message)> CreateParkingLotAsync(ParkingLotCreateRequest dto);
    Task<(int statusCode, object message)> UpdateParkingLotAsync(Guid id, ParkingLotCreateRequest dto);
    Task<(int statusCode, object message)> DeleteParkingLotAsync(Guid id);
    Task<(int statusCode, object message)> GetParkingLotAsync(Guid id);
    Task<(int statusCode, object message)> GetAllParkingLotsAsync();

    Task<(int statusCode, object message)> StartSessionAsync(Guid parkingLotId, string licensePlate, Guid userId);
    Task<(int statusCode, object message)> StopSessionAsync(Guid parkingLotId, string licensePlate, Guid userId);

    Task<(int statusCode, object message)> GetAllSessionsForLotAsync(Guid parkingLotId);

    Task<(int statusCode, object message)> GetSessionByIdAsync(Guid parkingLotId, Guid sessionId);

    Task<(int statusCode, object message)> DeleteSessionAsync(Guid parkingLotId, Guid sessionId);


}
