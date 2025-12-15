namespace v2.core.Interfaces;

using v2.Core.DTOs;

public interface IBilling
{

    Task<(int statusCode, object data)> GetMyInvoiceHistoryAsync(string identityUserId);

    Task<(int statusCode, object data)> GetInvoiceDetailsAsync(Guid invoiceId, string identityUserId);

    Task<(int statusCode, object data)> CreateBundleInvoiceAsync(CreateBundleInvoiceDto dto, string identityUserId);
    Task<(int statusCode, object data)> GetUserBillingSummaryAsync(string username, string identityUserId);
    Task<(int statusCode, object data)> GetMyMonthlyInvoiceHistoryAsync(int year, int month, string identityUserId);
}
