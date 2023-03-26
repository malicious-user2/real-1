// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YouRata.Common.Configuration;

namespace YouRata.ConflictMonitor.MilestoneInterface.ConfigurationValidation;

internal static class ConfigurationValidationExtensions
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
