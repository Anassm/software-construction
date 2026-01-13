using v2.core.Interfaces;
using v2.Core.DTOs;
using v2.Infrastructure.Data;
using v2.Core.Models;
namespace v2.infrastructure.Services;

using Microsoft.EntityFrameworkCore;

public class BillingService : IBilling
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
                    Id = i.ID,
                    InvoiceNumber = i.InvoiceNumber,
                    TotalAmount = i.TotalAmount,
                    DueDate = i.DueDate,
                    Status = i.Status.ToString()
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

    public async Task<(int statusCode, object data)> CreateBundleInvoiceAsync(CreateBundleInvoiceDto dto, string identityUserId)
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


            if (user.Role?.ToLower() != "admin" &&
                user.Role?.ToLower() != "employee" &&
                user.Role?.ToLower() != "business")
            {
                return (403, new { error = "Access denied. Business account required." });
            }


            if (dto.SessionIds == null || !dto.SessionIds.Any())
            {
                return (400, new { error = "At least one session ID is required." });
            }


            var sessions = await _db.Sessions
                .Where(s => dto.SessionIds.Contains(s.ID))
                .ToListAsync();

            if (sessions.Count != dto.SessionIds.Count)
            {
                var foundIds = sessions.Select(s => s.ID).ToList();
                var missingIds = dto.SessionIds.Except(foundIds).ToList();
                return (404, new
                {
                    error = "One or more sessions not found",
                    missingSessionIds = missingIds
                });
            }


            var alreadyInvoiced = await _db.Invoices
                .Where(i => i.Sessions.Any(s => dto.SessionIds.Contains(s.ID)))
                .Include(i => i.Sessions)
                .ToListAsync();

            if (alreadyInvoiced.Any())
            {
                var invoicedSessionIds = alreadyInvoiced
                    .SelectMany(i => i.Sessions)
                    .Where(s => dto.SessionIds.Contains(s.ID))
                    .Select(s => s.ID)
                    .ToList();

                return (409, new
                {
                    error = "One or more sessions are already invoiced",
                    alreadyInvoicedSessionIds = invoicedSessionIds,
                    existingInvoiceNumbers = alreadyInvoiced.Select(i => i.InvoiceNumber).ToList()
                });
            }


            float totalAmount = sessions.Sum(s => s.Price);


            var invoice = new Invoice
            {
                ID = Guid.NewGuid(),
                InvoiceNumber = GenerateInvoiceNumber(),
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow,
                DueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(14),
                Status = InvoiceStatus.Open,
                UserID = user.ID
            };


            foreach (var session in sessions)
            {
                invoice.Sessions.Add(session);
            }

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();


            var response = new BundleInvoiceResponseDto
            {
                InvoiceId = invoice.ID,
                InvoiceNumber = invoice.InvoiceNumber,
                TotalAmount = invoice.TotalAmount,
                CreatedAt = invoice.CreatedAt,
                DueDate = invoice.DueDate,
                Status = invoice.Status.ToString(),
                SessionCount = sessions.Count,
                BundledSessionIds = dto.SessionIds,
                CompanyName = dto.CompanyName
            };

            return (201, new
            {
                status = "Success",
                message = $"Bundle invoice created successfully with {sessions.Count} session(s).",
                invoice = response
            });
        }
        catch (Exception ex)
        {
            return (500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    private string GenerateInvoiceNumber()
    {
        return $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
    }



    public async Task<(int statusCode, object data)> GetUserBillingSummaryAsync(string username, string identityUserId)
    {
        try
        {

            var requestingUser = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

            if (requestingUser == null)
            {
                return (404, new { error = "Requesting user not found" });
            }


            if (requestingUser.Role?.ToLower() != "admin" && requestingUser.Role?.ToLower() != "employee")
            {
                return (403, new { error = "Access denied. Admin or employee role required." });
            }


            var targetUser = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (targetUser == null)
            {
                return (404, new { error = $"User '{username}' not found" });
            }


            var invoices = await _db.Invoices
                .Where(i => i.UserID == targetUser.ID)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new InvoiceSummaryDto
                {
                    Id = i.ID,
                    InvoiceNumber = i.InvoiceNumber,
                    TotalAmount = i.TotalAmount,
                    DueDate = i.DueDate,
                    Status = i.Status.ToString()
                })
                .ToListAsync();


            var payments = await _db.Payments
                .Where(p => p.Initiator == targetUser.Username)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentSummaryDto
                {
                    Id = p.ID,
                    Amount = p.Amount,
                    CreatedAt = p.CreatedAt,
                    CompletedAt = p.CompletedAt,
                    TransactionMethod = p.TransactionMethod,
                    TransactionIssuer = p.TransactionIssuer
                })
                .ToListAsync();


            var summary = new BillingSummaryDto
            {
                TotalInvoices = invoices.Count,
                TotalPaid = invoices.Count(i => i.Status == "Paid"),
                TotalOpen = invoices.Count(i => i.Status == "Open"),
                TotalOverdue = invoices.Count(i => i.Status == "Overdue"),
                TotalInvoicedAmount = invoices.Sum(i => i.TotalAmount)
            };


            var response = new UserBillingSummaryDto
            {
                User = new UserInfoDto
                {
                    Id = targetUser.ID,
                    Username = targetUser.Username,
                    Name = targetUser.Name,
                    Email = targetUser.Email ?? ""
                },
                Invoices = invoices,
                Payments = payments,
                Summary = summary
            };

            return (200, new { status = "Success", data = response });
        }
        catch (Exception ex)
        {
            return (500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    public async Task<(int statusCode, object data)> GetMyMonthlyInvoiceHistoryAsync(
    int year,
    int month,
    string identityUserId)
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
                .Where(i =>
                    i.UserID == user.ID &&
                    i.CreatedAt.Year == year &&
                    i.CreatedAt.Month == month
                )
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new InvoiceSummaryDto
                {
                    Id = i.ID,
                    InvoiceNumber = i.InvoiceNumber,
                    TotalAmount = i.TotalAmount,
                    DueDate = i.DueDate,
                    Status = i.Status.ToString()
                })
                .ToListAsync();

            var totalAmount = invoices.Sum(i => i.TotalAmount);

            return (200, new
            {
                status = "Success",
                period = $"{year}-{month:D2}",
                totalInvoices = invoices.Count,
                totalAmount,
                invoices
            });
        }
        catch (Exception ex)
        {
            return (500, new
            {
                error = "An unexpected error occurred.",
                details = ex.Message
            });
        }
    }
}