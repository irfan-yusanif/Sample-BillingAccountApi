namespace Sample.BillingAccount.Api.Model.Internal;

public class Transaction
{
    public TransactionType Type { get; set; } = new();
    public Price Value { get; set; } = new();
    public string CustomerId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}
