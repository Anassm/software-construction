using v2.Core.DTOs;
using v2.Core.Models;

namespace v2.core.Interfaces
{
    public interface IReservation
    {
    Task<Reservation> CreateReservationAsync(ReservationCreateRequest request, string identityUserId);
    Task<(Reservation? reservation, int statusCode, object? message)>
        UpdateReservationForUserAsync(Guid reservationId, string identityUserId, ReservationUpdateRequest request);
    Task<IEnumerable<Reservation>> GetReservationsForUserAsync(string identityUserId);
    Task<bool> DeleteReservationForUserAsync(Guid reservationId, string identityUserId);
    }
}