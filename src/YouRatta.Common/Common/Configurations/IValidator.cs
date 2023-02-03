using System;
using System.Threading.Tasks;

namespace YouRatta.Common.Configurations;

public interface IValidator
{
    Task ValidateAsync();
}
