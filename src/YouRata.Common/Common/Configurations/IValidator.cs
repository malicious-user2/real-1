using System;
using System.Threading.Tasks;

namespace YouRata.Common.Configurations;

public interface IValidator
{
    Task ValidateAsync();
}
