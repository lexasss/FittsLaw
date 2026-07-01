using System.Globalization;
using System.Windows.Controls;

namespace FittsLaw.Validators;

internal class NumberArrayValidationRule : ValidationRule
{
    public int Minimum { get; set; }
    public int Maximum { get; set; }
    public int? ExactCount { get; set; } = null;

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var parts = ((string)value).Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return new ValidationResult(false, $"At least one value is required.");
        }
        if (parts.All(p => double.TryParse(p, out var v) && v >= Minimum && v <= Maximum) != true)
        {
            return new ValidationResult(false, $"Please enter an array of numbers between {Minimum} and {Maximum}.");
        }
        if (ExactCount != null && ExactCount != parts.Length)
        {
            return new ValidationResult(false, $"Please enter exactly {ExactCount} numbers.");
        }

        return ValidationResult.ValidResult;
    }
}
