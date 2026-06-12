using System.Globalization;
using System.Windows.Controls;

namespace FittsLaw.Validators;

internal class DoubleRangeValidationRule : ValidationRule
{
    public double Minimum { get; set; }
    public double Maximum { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (!double.TryParse(value?.ToString(), out double number))
        {
            return new ValidationResult(false, "Enter a valid number.");
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