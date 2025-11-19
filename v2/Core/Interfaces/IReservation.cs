using v2.Core.DTOs;
using v2.Core.Models;

namespace v2.core.Interfaces;

public interface IReservation
{
    Task<Reservation> CreateReservationAsync(ReservationCreateRequest request);
    Task<IEnumerable<Reservation>> GetReservationsForUserAsync(string identityUserId);
    Task<bool> DeleteReservationForUserAsync(Guid reservationId, string identityUserId);
        Task<(Reservation? data, int statusCode, object? message)> UpdateReservationAsync(
        Guid id,
        ReservationUpdateRequest request,
        string identityUserId
    );
}
