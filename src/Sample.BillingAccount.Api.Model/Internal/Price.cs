namespace Sample.BillingAccount.Api.Model.Internal;

public class Price
{
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}
