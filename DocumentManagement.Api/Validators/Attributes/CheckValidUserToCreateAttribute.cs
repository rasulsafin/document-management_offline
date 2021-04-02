using System;
using System.ComponentModel.DataAnnotations;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Api.Validators
{
    /// <summary>
    /// Validation attribute intended to check if supplied user's values are valid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    internal sealed class CheckValidUserToCreateAttribute : ALocalizableValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var isValid = false;
            if (value is UserToCreateDto user)
            {
                isValid = user.Login != null
                    && user.Password != null;
            }
            else
            {
                throw new InvalidOperationException($"{nameof(CheckValidUserToCreateAttribute)} can validate only UserToCreateDto type");
            }

            return isValid ? ValidationResult.Success : GetLocalizedErrorResult(validationContext);
        }
    }
}
