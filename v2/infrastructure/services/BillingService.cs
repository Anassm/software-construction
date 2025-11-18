using v2.core.Interfaces;
using v2.Core.DTOs;
using v2.Infrastructure.Data;
namespace v2.infrastructure.Services;
using Microsoft.EntityFrameworkCore;


public class BillingService: IBilling
{
    private readonly ApplicationDbContext _db; 

    public BillingService(ApplicationDbContext db)
    {
        _db = db; 
    }

  public async Task<(int statusCode, object data)> GetPaymentHistoryAsync(Guid userId)
{
    try
    {
        var items = await _db.Payments
            .Where(p => p.Initiator == userId.ToString()) 
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentResponseDTO
            {
                ID                = p.ID,
                Amount            = p.Amount,
                Initiator         = p.Initiator,      
                CreatedAt         = p.CreatedAt,
                CompletedAt       = p.CompletedAt,
                Hash              = p.Hash,
                TransactionAmount = p.TransactionAmount,
                TransactionDate   = p.TransactionDate,
                TransactionMethod = p.TransactionMethod,
                TransactionIssuer = p.TransactionIssuer,
                TransactionBank   = p.TransactionBank,
                SessionID         = p.SessionID
            })
            .ToListAsync();

        return (200, new { items });
    }
    catch
    {
        return (500, new { error = "An unexpected error occurred." });
    }
}

}