using v2.Core.Interfaces;
using v2.Core.Models;
using v2.Core.DTOs;
using v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace v2.Infrastructure.Services
{
    public class PaymentService : IPayment
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(int statusCode, object data)> CreatePaymentAsync(CreatePaymentRequestDTO request, string initiatorIdentityId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.IdentityUserId == initiatorIdentityId);
            if (user == null)
            {
                return (404, new { error = $"User record not found for identityId {initiatorIdentityId}" });
            }

            if (request.SessionID == null && request.Transaction == null)
            {
                return (400, new { error = "Required field missing, field: transaction or sessionID" });
            }

            var payment = new Payment
            {
                Amount = request.Amount,
                Initiator = user.Username,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = null,
                Hash = null,
                TransactionAmount = request.TransactionAmount ?? request.Amount,
                TransactionDate = request.TransactionDate ?? DateTime.UtcNow,
                TransactionMethod = request.TransactionMethod ?? "N/A",
                TransactionIssuer = request.TransactionIssuer ?? "N/A",
                TransactionBank = request.TransactionBank ?? "N/A",
                SessionID = request.SessionID,
                OldID = request.Transaction ?? string.Empty
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            var responseDto = MapPaymentToDto(payment);
            var responseData = new { status = "Success", payment = responseDto };
            return (201, responseData);
        }

        public async Task<(int statusCode, object data)> ConfirmPaymentAsync(Guid paymentId, ConfirmPaymentRequestDTO dto, string initiatorIdentityId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return (404, new { error = "Payment not found" });
            }

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.IdentityUserId == initiatorIdentityId);
            if (user == null)
            {
                return (404, new { error = "User not found" });
            }

            if (payment.Initiator != user.Username && user.Role != "Admin")
            {
                return (403, new { error = "User forbidden from confirming this payment" });
            }

            if (dto.Validation == "invalid_hash")
            {
                return (401, new { error = "Validation failed", info = "The validation of the security hash could not be validated." });
            }

            payment.CompletedAt = DateTime.UtcNow;
            payment.Hash = dto.Validation;
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            var responseDto = MapPaymentToDto(payment);
            var responseData = new { status = "Success", payment = responseDto };
            return (200, responseData);
        }

        public async Task<(int statusCode, object data)> GetPaymentsByUserAsync(string? initiatorUsername, string identityId)
        {
            var requestingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.IdentityUserId == identityId);

            if (requestingUser == null)
            {
                return (404, new { error = "Requesting user record not found" });
            }

            string targetUsername;

            if (string.IsNullOrEmpty(initiatorUsername))
            {
                targetUsername = requestingUser.Username;
            }
            else
            {
                if (requestingUser.Role != "Admin")
                {
                    return (403, new { error = "Access denied. Admin role required to view other user's payments." });
                }
                targetUsername = initiatorUsername;
            }

            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.Initiator == targetUsername)
                .OrderByDescending(p => p.CreatedAt)
                .Select(payment => MapPaymentToDto(payment))
                .ToListAsync();

            if (payments == null || !payments.Any())
            {
                if (!string.IsNullOrEmpty(initiatorUsername))
                {
                    var targetUserExists = await _context.Users.AnyAsync(u => u.Username == targetUsername);
                    if (!targetUserExists)
                    {
                        return (404, new { error = $"User '{targetUsername}' not found." });
                    }
                }
                return (200, new List<PaymentResponseDTO>());
            }

            return (200, payments);
        }

        public async Task<(int statusCode, object data)> RefundPaymentAsync(RefundPaymentRequestDTO request, string adminIdentityId)
        {
            var adminUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.IdentityUserId == adminIdentityId);
            if (adminUser == null || adminUser.Role != "Admin")
            {
                return (403, new { error = "Access denied. Admin role required for refunds." });
            }

            var payment = await _context.Payments.FindAsync(request.PaymentId);
            if (payment == null)
            {
                return (404, new { error = $"Payment with ID {request.PaymentId} not found." });
            }

            if (payment.Hash != null && payment.Hash.StartsWith("REFUND:"))
            {
                return (409, new { error = "Payment has already been refunded." });
            }

            payment.CompletedAt = DateTime.UtcNow;
            payment.Hash = $"REFUND:{request.Reason ?? "N/A"}:{DateTime.UtcNow:yyyMMddHHmmss}";

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            var responseDto = MapPaymentToDto(payment);
            var responseData = new { status = "Success", payment = responseDto };
            return (201, responseData);
        }

        public async Task<(int statusCode, object data)> UpdatePaymentAsync(Guid paymentId, UpdatePaymentRequestDTO dto, string identityId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return (404, new { error = "Payment not found" });
            }

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            if (user == null || (payment.Initiator != user.Username && user.Role != "Admin"))
            {
                return (403, new { error = "Access denied." });
            }

            if (dto.Amount.HasValue) payment.Amount = dto.Amount.Value;
            if (dto.TransactionAmount.HasValue) payment.TransactionAmount = dto.TransactionAmount.Value;
            if (dto.TransactionDate.HasValue) payment.TransactionDate = dto.TransactionDate.Value;
            if (!string.IsNullOrWhiteSpace(dto.TransactionMethod)) payment.TransactionMethod = dto.TransactionMethod;
            if (!string.IsNullOrWhiteSpace(dto.TransactionIssuer)) payment.TransactionIssuer = dto.TransactionIssuer;
            if (!string.IsNullOrWhiteSpace(dto.TransactionBank)) payment.TransactionBank = dto.TransactionBank;
            if (dto.CompletedAt.HasValue) payment.CompletedAt = dto.CompletedAt.Value;
            if (!string.IsNullOrWhiteSpace(dto.Hash)) payment.Hash = dto.Hash;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            var responseDto = MapPaymentToDto(payment);
            var responseData = new { status = "Success", payment = responseDto };
            return (200, responseData);
        }

        private PaymentResponseDTO MapPaymentToDto(Payment payment)
        {
            return new PaymentResponseDTO
            {
                ID = payment.ID,
                Amount = payment.Amount,
                Initiator = payment.Initiator,
                CreatedAt = payment.CreatedAt,
                CompletedAt = payment.CompletedAt,
                Hash = payment.Hash,
                TransactionAmount = payment.TransactionAmount,
                TransactionDate = payment.TransactionDate,
                TransactionMethod = payment.TransactionMethod,
                TransactionIssuer = payment.TransactionIssuer,
                TransactionBank = payment.TransactionBank,
                SessionID = payment.SessionID
            };
        }
    }
}