namespace Sample.BillingAccount.Api.Model.Internal;

public class CustomerAccount
{
    public string CustomerId { get; set; } = string.Empty;
    public CreditAccount CreditAccount { get; set; } = new();
}

public class CreditAccount
{
    public Price Balance { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();
}

public class Statement
{
    public Price Balance { get; set; } = new();
    public DateOnly Date { get; set; }
    public List<SingleTransaction> Transactions { get; set; } = new();
}

public class SingleTransaction
{
    public TransactionType Type { get; set; } = new();
    public Price Value { get; set; } = new();
}
