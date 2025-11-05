using v2.Core.DTOs;
using v2.Core.Models;
using System.Collections.Generic;

namespace v2.core.Interfaces;

public interface IReservation
{
    Task<Reservation> CreateReservationAsync(ReservationCreateRequest request);
    Task<IEnumerable<Reservation>> GetReservationsAsync(Guid userId);
}