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

    
    public async Task<(int statusCode, object data)> GetMyInvoiceHistoryAsync(string identityUserId)
    {
        try
        {
           
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

           
            if (user == null)
            {
                return (404, new { error = "User not found" });
            }

    
            var invoices = await _db.Invoices
                .Where(i => i.UserID == user.ID)               
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

           
            return (200, new { status = "Success", invoices });
        }
        catch (Exception ex)
        {
            
            return (500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }
}