using AutoMapper;
using Sample.BillingAccount.Api.Model.Dto;
using Sample.BillingAccount.Api.Model.Request;

namespace Sample.BillingAccount.Api.Mappers;

public class CreditAccountProfile : Profile
{
    public CreditAccountProfile()
    {
        CreateMap<TransactionsRequest, TransactionsDto>()
            .ForMember(des => des.Transactions,
                opt => opt.MapFrom(s => s.Transactions)).ReverseMap();
    }
}
