using v2.Core.DTOs;
using v2.Core.Models;

namespace v2.Core.Interfaces;

public interface IReservation
{
    Task<Reservation> CreateReservationAsync(ReservationCreateRequest request);
}
