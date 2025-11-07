using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using v2.Core.DTOs;

namespace v2.Core.Interfaces
{
    public interface IPayments
    {
        // 5.5.1 Start/create a new Payment
        Task<PaymentResponseDTO> CreatePaymentAsync(CreatePaymentRequestDTO request);

        // 5.5.2: Confirm a payment via hash
        Task<PaymentResponseDTO> ConfirmPaymentAsync(Guid paymentId, string hash);

        // 5.5.3 Get payments for a user/initiator
        Task<IEnumerable<PaymentResponseDTO>> GetPaymentsByUserAsync(string initiator);

        // 5.5.4 Update an incomplete payment (partial update)
        Task<PaymentResponseDTO> UpdatePaymentAsync(Guid paymentId, UpdatePaymentRequestDTO updateData);

        // 5.5.5 Refund a payment (put reason in hash for now)
        Task<PaymentResponseDTO> RefundPaymentAsync(Guid paymentId, string? reason);
    }
}
