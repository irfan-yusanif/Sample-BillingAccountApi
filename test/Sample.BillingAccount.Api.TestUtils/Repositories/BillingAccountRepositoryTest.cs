using Sample.BillingAccount.Api.Model.Dto;
using Sample.BillingAccount.Api.Model.Internal;
using Sample.BillingAccount.Api.Repositories;

namespace Sample.BillingAccount.Api.TestUtils.Repositories;

public class BillingAccountRepositoryTest : IBillingAccountRepository
{
    public Task SaveBillingAccounts(List<CustomerAccount> customerBillingAccounts)
    {
        return Task.CompletedTask;
    }

    public Task<List<CustomerAccount>> GetAccountAndAddTransaction(TransactionsDto transactionsDto)
    {
        var list = new List<CustomerAccount>();
        return Task.FromResult(list);
    }

    public Task<CustomerAccount> GetCustomerAccount(string customerId)
    {
        return Task.FromResult(new CustomerAccount());
    }
}
