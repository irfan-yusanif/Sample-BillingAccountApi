using Sample.BillingAccount.Api.Model.Internal;

namespace Sample.BillingAccount.Api.Model.Request;

public class TransactionsRequest
{
    public List<Transaction> Transactions { get; set; } = new();
}
