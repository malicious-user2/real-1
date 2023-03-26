// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;

namespace YouRata.Common.Configuration;

public interface IValidator
{
    Task ValidateAsync();
}
