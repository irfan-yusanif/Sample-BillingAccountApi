using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Sample.BillingAccount.Api.Filters;
using Sample.BillingAccount.Api.Model.Dto;
using Sample.BillingAccount.Api.Model.Request;
using Sample.BillingAccount.Api.Providers;
using Sample.BillingAccount.Api.Repositories;

namespace Sample.BillingAccount.Api.Controllers;

[ApiController]
[Route("[controller]")]
[ValidateRequestExtraProperties]
public class BillingAccountController : ControllerBase
{
    private readonly ILogger<BillingAccountController> _logger;
    private readonly IMapper _mapper;
    private readonly IBillingAccountRepository _repository;
    private readonly IRequestHeaderProvider _requestHeaderProvider;

    public BillingAccountController(
        ILogger<BillingAccountController> logger,
        IMapper mapper,
        IBillingAccountRepository repository,
        IRequestHeaderProvider requestHeaderProvider)
    {
        _logger = logger;
        _mapper = mapper;
        _repository = repository;
        _requestHeaderProvider = requestHeaderProvider;
    }

    [HttpPut]
    [Route("transactions")]
    [ValidateBillingAccountRequest]
    public async Task<IActionResult> Save(TransactionsRequest request)
    {
        var transactionsDto = _mapper.Map<TransactionsDto>(request);

        var customerAccounts = await _repository.GetAccountAndAddTransaction(transactionsDto);
        await _repository.SaveBillingAccounts(customerAccounts);

        return NoContent();
    }

    [HttpGet]
    [Route("{customerId}")]
    [ValidateBillingAccountRequest]
    public async Task<IActionResult> GetCustomerAccount(string customerId)
    {
        var customerAccount = await _repository.GetCustomerAccount(customerId);

        return Ok(customerAccount);
    }
}
