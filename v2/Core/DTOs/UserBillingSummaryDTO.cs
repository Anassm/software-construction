namespace v2.Core.DTOs;

public class UserBillingSummaryDto
{
    public UserInfoDto User { get; set; }
    public List<InvoiceSummaryDto> Invoices { get; set; }
    public List<PaymentSummaryDto> Payments { get; set; }
    public BillingSummaryDto Summary { get; set; }
}

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class PaymentSummaryDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string TransactionMethod { get; set; }
    public string TransactionIssuer { get; set; }
}

public class BillingSummaryDto
{
    public int TotalInvoices { get; set; }
    public int TotalPaid { get; set; }
    public int TotalOpen { get; set; }
    public int TotalOverdue { get; set; }
    public float TotalInvoicedAmount { get; set; }
   
}