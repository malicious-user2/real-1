using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YouRatta.Common.Configurations;

public class ConfigurationsValidator : IValidator
{
    private readonly IEnumerable<IValidatableConfiguration> _validatableObjects;

    public ConfigurationsValidator(IEnumerable<IValidatableConfiguration> validatableObjects)
    {
        _validatableObjects = validatableObjects;
    }

    public async Task ValidateAsync()
    {
        if (_validatableObjects == null) return;
        foreach (IValidatableConfiguration validatableObject in _validatableObjects)
        {
            await Task.Run(() => validatableObject.Validate()).ConfigureAwait(false);
            if (validatableObject is YouRattaConfiguration)
            {
                await Task.Run(() =>
                {
                    ((YouRattaConfiguration)validatableObject).ValidateConfigurationMembers();
                }).ConfigureAwait(false);
            }
        }
    }
}
