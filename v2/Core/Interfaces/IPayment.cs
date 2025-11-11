using v2.Core.DTOs;

namespace v2.Core.Interfaces
{
    public interface IPayment
    {
        Task<(int statusCode, object data)> CreatePaymentAsync(CreatePaymentRequestDTO request, string initiatorIdentityId);
        Task<(int statusCode, object data)> ConfirmPaymentAsync(Guid paymentId, ConfirmPaymentRequestDTO dto, string initiatorIdentityId);
        Task<(int statusCode, object data)> GetPaymentsByUserAsync(string? initiatorUsername, string identityId);
        Task<(int statusCode, object data)> RefundPaymentAsync(RefundPaymentRequestDTO request, string adminIdentityId);
        Task<(int statusCode, object data)> UpdatePaymentAsync(Guid paymentId, UpdatePaymentRequestDTO dto, string identityId);
    }
}