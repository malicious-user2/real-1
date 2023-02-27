using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouRata.Common.Configurations;

public abstract class BaseValidatableConfiguration : IValidatableConfiguration
{
    public void Validate()
    {
        List<ValidationResult> errors = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(this, new ValidationContext(this), errors, true);

        if (!isValid)
        {
            throw new AggregateException(errors.Select(e => new ValidationException(e.ErrorMessage)));
        }
    }
}
