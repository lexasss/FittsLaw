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
        var setup = ExperimentSetup.From(new Setup
        {
            TargetCount = 5,
            Amplitudes = amplitudes,
            Widths = amplitudes
        });

        Assert.Equal(expected, setup.Amplitudes);
        Assert.Equal(expected, setup.Widths);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    public void FromRejectsInvalidAmplitudeValues(string amplitudes)
    {
        var vm = new Setup
        {
            TargetCount = 5,
            Amplitudes = amplitudes,
            Widths = "10"
        };

        Assert.Throws<ArgumentException>(() => ExperimentSetup.From(vm));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    public void FromRejectsInvalidWidthValues(string widths)
    {
        var vm = new Setup
        {
            TargetCount = 5,
            Amplitudes = "100",
            Widths = widths
        };

        Assert.Throws<ArgumentException>(() => ExperimentSetup.From(vm));
    }
}
