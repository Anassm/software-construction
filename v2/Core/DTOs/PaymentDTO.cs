using System.ComponentModel.DataAnnotations;

namespace v2.Core.DTOs;

public class CreatePaymentRequestDTO
{
    public string? Transaction { get; set; }

    public decimal Amount { get; set; }

    public decimal? TransactionAmount { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? TransactionMethod { get; set; }
    public string? TransactionIssuer { get; set; }
    public string? TransactionBank { get; set; }
    public Guid? SessionID { get; set; }
}

public class PaymentResponseDTO
{
    public Guid ID { get; set; }
    public decimal Amount { get; set; }
    public string Initiator { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Hash { get; set; }
    public decimal TransactionAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionMethod { get; set; }
    public string TransactionIssuer { get; set; }
    public string TransactionBank { get; set; }
    public Guid? SessionID { get; set; }
}

public class UpdatePaymentRequestDTO
{
    public decimal? Amount { get; set; }
    public decimal? TransactionAmount { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? TransactionMethod { get; set; }
    public string? TransactionIssuer { get; set; }
    public string? TransactionBank { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Hash { get; set; }
}

public class ConfirmPaymentRequestDTO
{
    [Required]
    public object T_Data { get; set; }
    // Hash
    [Required]
    public string Validation { get; set; }
}

public class RefundPaymentRequestDTO
{
    [Required]
    public Guid PaymentId { get; set; }

    public string? Reason { get; set; }
}