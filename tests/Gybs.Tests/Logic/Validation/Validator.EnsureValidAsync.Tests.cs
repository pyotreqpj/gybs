using FluentAssertions;
using Gybs.DependencyInjection.Services;
using Gybs.Extensions;
using Gybs.Logic.Validation;
using Gybs.Logic.Validation.Internal;
using Gybs.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Gybs.Tests.Logic.Validation;

public class ValidatorEnsureValidAsyncTests
{
    private const string ServiceAttributeGroup = nameof(ValidatorEnsureValidAsyncTests);

    [Fact]
    public async Task ForSuccessfulValidationShouldPass()
    {
        var validator = CreateValidator();

        await validator
            .Require<SucceededRule>().WithData(string.Empty)
            .Require<SucceededRule>().WithData(string.Empty)
            .EnsureValidAsync();
    }

    [Fact]
    public async Task ForFailedValidationShouldThrowException()
    {
        var validator = CreateValidator();

        Func<Task> action = async () => await validator
            .Require<SucceededRule>().WithData(string.Empty)
            .Require<FailedRule>().WithData(string.Empty)
            .EnsureValidAsync();

        await action.Should().ThrowAsync<ValidationFailedException>();
    }

    private IValidator CreateValidator()
    {
        var logger = Substitute.For<ILogger<Validator>>();
        var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(
            new ServiceCollection()
                .AddGybs(builder => builder.AddValidator().AddAttributeServices(group: ServiceAttributeGroup))
        );

        return new Validator(logger, serviceProvider);
    }

    [TransientService((ServiceAttributeGroup))]
    private class SucceededRule : IValidationRule<string>
    {
        public Task<IResult> ValidateAsync(string data)
        {
            return Result.Success().ToCompletedTask();
        }
    }

    [TransientService((ServiceAttributeGroup))]
    private class FailedRule : IValidationRule<string>
    {
        public Task<IResult> ValidateAsync(string data)
        {
            return Result.Failure(new ResultErrorsDictionary().Add("key", "value")).ToCompletedTask();
        }
    }
}
