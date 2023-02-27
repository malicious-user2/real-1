using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YouRata.Common.Configurations;

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
            if (validatableObject is YouRataConfiguration)
            {
                await Task.Run(() =>
                {
                    ((YouRataConfiguration)validatableObject).ValidateConfigurationMembers();
                }).ConfigureAwait(false);
            }
        }
    }
}
