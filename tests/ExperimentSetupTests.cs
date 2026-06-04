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
            Widths = amplitudes,
            InputType = "MouseInput"
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
            Widths = "10",
            InputType = "MouseInput"
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
            Widths = widths,
            InputType = "MouseInput"
        };

        Assert.Throws<ArgumentException>(() => ExperimentSetup.From(vm));
    }

    [Fact]
    public void FromStoresSelectedInputType()
    {
        var setup = ExperimentSetup.From(new Setup
        {
            TargetCount = 5,
            Amplitudes = "100",
            Widths = "20",
            InputType = "TouchInput"
        });

        Assert.Equal("TouchInput", setup.InputType);
    }
}
