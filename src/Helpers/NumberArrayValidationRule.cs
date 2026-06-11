using System.Globalization;
using System.Windows.Controls;

namespace FittsLaw.Helpers;

internal class NumberArrayValidationRule : ValidationRule
{
    public int Minimum { get; set; }
    public int Maximum { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var parts = value?.ToString()?.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts?.All(p => double.TryParse(p, out var v) && v >= Minimum && v <= Maximum) != true)
        {
            return new ValidationResult(false, $"Please enter an array of numbers between {Minimum} and {Maximum}.");
        }

        return ValidationResult.ValidResult;
    }
}
