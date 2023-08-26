using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using MELT;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Sample.BillingAccount.Api.Model.Internal;

namespace Sample.BillingAccount.Api.TestUtils.AutoData;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() =>
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
        fixture.Customize<BindingInfo>(c => c.OmitAutoProperties());
        fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));
        fixture.Customize<Transaction>(composer => composer.FromFactory<Price, string, DateOnly>(CustomizeTransaction).OmitAutoProperties());
        fixture.Customizations.Add(new LoggerSpecimenBuilder());
        fixture.Register(TestLoggerFactory.Create);

        return fixture;
    })
    { }

    private static Transaction CustomizeTransaction(Price value, string customerId, DateOnly date)
    {
        var model = new Transaction
        {
            CustomerId = customerId,
            Value = value,
            Date = date,
            Type = Model.TransactionType.Charge
        };
        return model;
    }
}
