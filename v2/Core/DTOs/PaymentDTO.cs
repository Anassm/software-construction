using System.ComponentModel.DataAnnotations;

namespace v2.Core.DTOs;

public class CreatePaymentRequestDTO
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public string Initiator { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TransactionAmount { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    public string TransactionMethod { get; set; }

    [Required]
    public string TransactionIssuer { get; set; }

    [Required]
    public string TransactionBank { get; set; }

    [Required]
    public Guid SessionID { get; set; }
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
    public Guid SessionID { get; set; }
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
    public object T_Data { get; set; } // V1 gebruikt 't_data' als een generiek data object
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