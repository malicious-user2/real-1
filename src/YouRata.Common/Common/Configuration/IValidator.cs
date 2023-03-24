using System.Threading.Tasks;

namespace YouRata.Common.Configuration;

public interface IValidator
{
    Task ValidateAsync();
}
