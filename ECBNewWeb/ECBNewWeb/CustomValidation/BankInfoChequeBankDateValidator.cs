using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECBNewWeb.CustomValidation
{
    public class BankInfoChequeBankDateValidator : ValidationAttribute
    {
        private readonly string _ComarisonProperty;
        public BankInfoChequeBankDateValidator(string CompraisonProberty)
        {
            _ComarisonProperty = CompraisonProberty;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_ComarisonProperty);
            if (property == null)
                throw new ArgumentException("Property with this name not found");
            var ComparisonValue = property.GetValue(validationContext.ObjectInstance);
            if (ComparisonValue.ToString() == "true")
            {
                if (Convert.ToString(value) == string.Empty)
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }
            }
            return ValidationResult.Success;
            //return base.IsValid(value, validationContext);
        }
    }
}