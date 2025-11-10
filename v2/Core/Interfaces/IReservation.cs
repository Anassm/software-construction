using v2.Core.DTOs;
using v2.Core.Models;

namespace v2.core.Interfaces;

public interface IReservation
{
    Task<Reservation> CreateReservationAsync(ReservationCreateRequest request);
    Task<IEnumerable<Reservation>> GetReservationsForUserAsync(string identityUserId);
}
