using Sample.BillingAccount.Api.Model.Internal;

namespace Sample.BillingAccount.Api.Model.Dto;

public class TransactionsDto
{
    public List<Transaction> Transactions { get; set; } = new();
}
