using System.Globalization;
using System.Windows.Controls;

namespace FittsLaw.Helpers;

internal class IntegerRangeValidationRule : ValidationRule
{
    public int Minimum { get; set; }
    public int Maximum { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (!int.TryParse(value?.ToString(), out int number))
        {
            return new ValidationResult(false, "Enter a valid integer.");
        }

        if (number < Minimum || number > Maximum)
        {
            return new ValidationResult(
                false,
                $"Value must be between {Minimum} and {Maximum}.");
        }

        return ValidationResult.ValidResult;
    }
}