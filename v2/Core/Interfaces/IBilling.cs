namespace v2.core.Interfaces;

using v2.Core.DTOs;
public interface IBilling
{
    Task<(int statusCode, object data)> GetPaymentHistoryAsync(Guid userId);
}