using v2.Core.Interfaces;
using v2.Core.Models;
using v2.Core.DTOs;
using v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace v2.Infrastructure.Services;

public class PaymentService : IPayment
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentResponseDTO> CreatePaymentAsync(CreatePaymentRequestDTO request)
    {
        var payment = new Payment
        {
            Amount = request.Amount,
            Initiator = request.Initiator,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = null,
            TransactionAmount = request.TransactionAmount,
            TransactionDate = request.TransactionDate,
            TransactionMethod = request.TransactionMethod,
            TransactionIssuer = request.TransactionIssuer,
            TransactionBank = request.TransactionBank,
            SessionID = request.SessionID,
            Session = null
        };

        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

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

    public async Task<PaymentResponseDTO> ConfirmPaymentAsync(Guid paymentId, string hash)
    {
        if (hash == null || string.IsNullOrWhiteSpace(hash))
        {
            return null;
        }

        var payment = await _context.Payments.FirstOrDefaultAsync(payment => payment.ID == paymentId);
        if (payment == null)
        {
            return null;
        }

        payment.Hash = hash;
        payment.CompletedAt = DateTime.UtcNow;

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();

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

    public async Task<IEnumerable<PaymentResponseDTO>> GetPaymentsByUserAsync(string initiator)
    {
        if (initiator == null || string.IsNullOrWhiteSpace(initiator))
        {
            return Enumerable.Empty<PaymentResponseDTO>();
        }

        var query = _context.Payments
            .AsNoTracking()
            .Where(p => p.Initiator == initiator)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        var list = await query.ToListAsync();
        return list.Select(payment => new PaymentResponseDTO
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
        });
    }

    public async Task<PaymentResponseDTO> UpdatePaymentAsync(Guid paymentId, UpdatePaymentRequestDTO updateData)
    {
        if (updateData == null)
            return null;

        var payment = await _context.Payments.FirstOrDefaultAsync(payment => payment.ID == paymentId);
        if (payment == null)
            return null;

        // Apply only the properties from which updateData has values for.
        if (updateData.Amount.HasValue)
            payment.Amount = updateData.Amount.Value;

        if (updateData.TransactionAmount.HasValue)
            payment.TransactionAmount = updateData.TransactionAmount.Value;

        if (updateData.TransactionDate.HasValue)
            payment.TransactionDate = updateData.TransactionDate.Value;

        if (!string.IsNullOrWhiteSpace(updateData.TransactionMethod))
            payment.TransactionMethod = updateData.TransactionMethod!;

        if (!string.IsNullOrWhiteSpace(updateData.TransactionIssuer))
            payment.TransactionIssuer = updateData.TransactionIssuer!;

        if (!string.IsNullOrWhiteSpace(updateData.TransactionBank))
            payment.TransactionBank = updateData.TransactionBank!;

        if (updateData.CompletedAt.HasValue)
            payment.CompletedAt = updateData.CompletedAt.Value;

        if (!string.IsNullOrWhiteSpace(updateData.Hash))
            payment.Hash = updateData.Hash;

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();

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

    public async Task<PaymentResponseDTO> RefundPaymentAsync(Guid paymentId, string? reason)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(payment => payment.ID == paymentId);

        if (payment == null)
        {
            return null;
        }

        payment.CompletedAt = DateTime.UtcNow;
        // Temporary workaround.. Storing a refund reasoning string inside hash, should probably have completed flag
        var reviewedReason = string.IsNullOrWhiteSpace(reason) ? "refund" : reason!;
        payment.Hash = $"REFUND:{reason}:{DateTime.UtcNow:yyyMMddHHmmss}";

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();

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
