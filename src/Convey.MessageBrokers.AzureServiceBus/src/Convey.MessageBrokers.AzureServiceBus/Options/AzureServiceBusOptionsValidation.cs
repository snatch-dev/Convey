using Microsoft.Extensions.Options;

namespace Convey.MessageBrokers.AzureServiceBus.Options;

internal sealed class AzureServiceBusOptionsValidation : IValidateOptions<AzureServiceBusOptions>
{
    public ValidateOptionsResult Validate(string name, AzureServiceBusOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            errors.Add($"A service name must be provided on the {nameof(AzureServiceBusOptions)}");
        }

        return errors.Count > 1
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}