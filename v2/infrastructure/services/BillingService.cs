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

 public async Task<(int statusCode, object data)> GetInvoiceDetailsAsync(Guid invoiceId, string identityUserId)
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

       
        if (user.Role?.ToLower() != "admin" && user.Role?.ToLower() != "employee")
        {
            return (403, new { error = "Access denied. Admin or employee role required." });
        }

    
        var invoice = await _db.Invoices
            .Include(i => i.User)
            .Include(i => i.Sessions)
                .ThenInclude(s => s.ParkingLot)
            .FirstOrDefaultAsync(i => i.ID == invoiceId);

        if (invoice == null)
        {
            return (404, new { error = "Invoice not found" });
        }

        var invoiceDetails = new
        {
            id = invoice.ID,
            invoiceNumber = invoice.InvoiceNumber,
            totalAmount = invoice.TotalAmount,
            createdAt = invoice.CreatedAt,
            dueDate = invoice.DueDate,
            status = invoice.Status.ToString(),
            customer = new
            {
                id = invoice.User.ID,
                username = invoice.User.Username,
                name = invoice.User.Name,
                email = invoice.User.Email
            },
            sessions = invoice.Sessions.Select(s => new
            {
                id = s.ID,
                licensePlate = s.LicensePlate,
                startTime = s.StartTime,
                endTime = s.EndTime,
                duration = s.duration,
                price = s.Price,
                paymentStatus = s.PaymentStatus.ToString(),
                parkingLot = s.ParkingLot != null ? new
                {
                    id = s.ParkingLot.ID,
                    name = s.ParkingLot.Name,
                    location = s.ParkingLot.Location
                } : null
            }).ToList()
        };

        return (200, new { status = "Success", invoice = invoiceDetails });
    }
    catch (Exception ex)
    {
        return (500, new { error = "An unexpected error occurred.", details = ex.Message });
    }
}
}