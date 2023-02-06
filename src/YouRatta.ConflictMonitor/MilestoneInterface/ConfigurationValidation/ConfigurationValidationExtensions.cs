using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YouRatta.Common.Configurations;

namespace YouRatta.ConflictMonitor.MilestoneInterface.ConfigurationValidation;

public static class ConfigurationValidationExtensions
{
    public static async Task ValidateConfigurationAsync(this IHost host)
    {
        IEnumerable<IValidator>? validators = host.Services.GetService<IEnumerable<IValidator>>();
        if (validators != null)
        {
            foreach (IValidator validator in validators)
            {
                await validator.ValidateAsync().ConfigureAwait(false);
            }
        }
    }
}
