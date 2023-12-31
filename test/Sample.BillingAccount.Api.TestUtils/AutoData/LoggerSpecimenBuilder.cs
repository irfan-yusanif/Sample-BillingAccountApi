﻿using System.Reflection;
using AutoFixture.Kernel;
using MELT;
using Microsoft.Extensions.Logging;

namespace Sample.BillingAccount.Api.TestUtils.AutoData;

public class LoggerSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is not ParameterInfo parameterInfo || !parameterInfo.ParameterType.IsGenericType)
            return new NoSpecimen();

        var genericTypeDefinition = parameterInfo.ParameterType.GetGenericTypeDefinition();
        if (genericTypeDefinition != typeof(ILogger<>))
            return new NoSpecimen();

        var loggerFactory = context.Resolve(typeof(ITestLoggerFactory)) as ITestLoggerFactory ??
                            TestLoggerFactory.Create();
        var categoryType = parameterInfo.ParameterType.GetGenericArguments()[0];

        var createLoggerMethod = typeof(LoggerFactoryExtensions).GetMethods().Single(x =>
            x.IsGenericMethod && x.Name == nameof(LoggerFactoryExtensions.CreateLogger));
        var genericCreateLoggerMethod = createLoggerMethod.MakeGenericMethod(categoryType);
        var genericLogger = genericCreateLoggerMethod.Invoke(null, new object?[] { loggerFactory });

        return genericLogger!;
    }
}
