using System.Windows;
using FittsLaw.Models;
using FittsLaw.Services;
using Target = FittsLaw.Models.Target;

namespace FittsLaw.Tests;

public class StatisticsTests
{
    [Fact]
    public void ComputeCalculatesMovementTimeAndErrors()
    {
        var block = new Block(0, 0, amplitude: 100, width: 20)
        {
            Targets =
            [
                new Target
                {
                    Position = new Point(0, 0),
                    ActivationOffset = new Point(0, 0),
                    ActivationTimestamp = 100
                },
                new Target
                {
                    Position = new Point(100, 0),
                    ActivationOffset = new Point(11, 0),
                    ActivationTimestamp = 350
                },
                new Target
                {
                    Position = new Point(0, 0),
                    ActivationOffset = new Point(0, 0),
                    ActivationTimestamp = 650
                }
            ]
        };

        var result = Statistics.Compute([block]);

        Assert.Equal("275", result["MT, ms"][0]);
        Assert.Equal("1", result["Errors"][0]);
        Assert.Equal("50.0", result["Errors, %"][0]);
    }

    [Fact]
    public void ComputeHandlesBlockWithFewerThanTwoTargets()
    {
        var block = new Block(0, 0, amplitude: 100, width: 20)
        {
            Targets =
            [
                new Target
                {
                    Position = new Point(0, 0),
                    ActivationTimestamp = 100
                },
                new Target
                {
                    Position = new Point(100, 0),
                    ActivationTimestamp = 1000
                }
            ]
        };

        var result = Statistics.Compute([block]);

        Assert.Equal("1", result["Trials"][0]);
        Assert.Equal("900", result["MT, ms"][0]);
        Assert.NotEqual("0", result["Throughput, b/s"][0]);
    }

    [Fact]
    public void ComputeHandlesZeroEffectiveWidthWithoutInfinityOrNaN()
    {
        var block = new Block(0, 0, amplitude: 100, width: 20)
        {
            Targets =
            [
                new Target
                {
                    Position = new Point(0, 0),
                    ActivationTimestamp = 100
                },
                new Target
                {
                    Position = new Point(100, 0),
                    ActivationTimestamp = 300
                }
            ]
        };

        var result = Statistics.Compute([block]);

        Assert.Equal("0.0", result["Eff. Width, px"][0]);
        Assert.Equal("0.00", result["Eff. ID, bits"][0]);
    }
}
