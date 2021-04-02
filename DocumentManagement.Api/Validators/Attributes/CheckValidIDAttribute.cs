using System;
using System.ComponentModel.DataAnnotations;

namespace MRS.DocumentManagement.Api.Validators
{
    /// <summary>
    /// Validation attribute intended to check if supplied ID values are valid. Does NOT check if entity with supplied ID actually exists.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    internal sealed class CheckValidIDAttribute : ALocalizableValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var isValid = false;
            if (value is ID<object> id)
            {
                isValid = id.IsValid;
            }
            else if (value is int intID)
            {
                isValid = intID > 0;
            }
            else
            {
                throw new InvalidOperationException($"{nameof(CheckValidIDAttribute)} can validate only int or ID<T> type");
            }

            return isValid ? ValidationResult.Success : GetLocalizedErrorResult(validationContext);
        }
    }
}
