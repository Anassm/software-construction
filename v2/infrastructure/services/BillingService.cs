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

  public async Task<(int statusCode, object data)> GetInvoiceHistoryAsync(Guid userId)
{
    try
    {
        var invoices = await _db.Invoices
            .Where(i => i.UserID == userId)               
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InvoiceSummaryDto
            {
                Id            = i.ID,
                InvoiceNumber = i.InvoiceNumber,
                TotalAmount   = i.TotalAmount,
                DueDate       = i.DueDate,
                Status        = i.Status.ToString()
            })
            .ToListAsync();

        return (200, new { invoices });
    }
    catch
    {
        return (500, new { error = "An unexpected error occurred." });
    }
}


}