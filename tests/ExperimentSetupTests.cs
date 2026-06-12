using FittsLaw.Models;
using FittsLaw.ViewModels;

namespace FittsLaw.Tests;

public class ExperimentSetupTests
{
    [Theory]
    [InlineData("100 200", new[] { 100.0, 200.0 })]
    [InlineData("100,200", new[] { 100.0, 200.0 })]
    [InlineData("100, 200", new[] { 100.0, 200.0 })]
    public void FromAcceptsSpaceAndCommaSeparatedNumbers(string amplitudes, double[] expected)
    {
        var values = ExperimentSetup.ToNumbers<double>(amplitudes);
        Assert.Equal(expected, values);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    public void FromRejectsInvalidValues(string amplitudes)
    {
        Assert.Throws<ArgumentException>(() => ExperimentSetup.ToNumbers<double>(amplitudes));
    }
}
