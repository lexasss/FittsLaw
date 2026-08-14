using FittsLaw.Models;
using FittsLaw.Services;

namespace FittsLaw.Tests;

public class LayoutCreatorTest
{
    [Fact]
    public void CreateBuildsExpectedTargetCountAndSize()
    {
        StaThread.Run(() =>
        {
            var block = new Block(0, 0, amplitude: 120, width: 24);

            var targets = LayoutCreator.CreateCircular(block, targetCount: 7, fieldSize: 168);

            Assert.Equal(7, targets.Length);
            Assert.All(targets, target =>
            {
                Assert.Equal(24, target.Size);
                Assert.Equal(24, target.Size);
            });
        });
    }

    [Fact]
    public void CreatePlacesTargetsWithinParentBounds()
    {
        StaThread.Run(() =>
        {
            var block = new Block(0, 0, amplitude: 120, width: 24);
            var fieldSize = block.Amplitude + 2 * block.Width;

            var targets = LayoutCreator.CreateCircular(block, targetCount: 7, fieldSize);

            Assert.All(targets, target =>
            {
                Assert.InRange(target.Position.X, 0, fieldSize);
                Assert.InRange(target.Position.Y, 0, fieldSize);
            });
        });
    }

    [Fact]
    public void CreatePlacesConsecutiveTargetsAtBlockAmplitudeDistance()
    {
        StaThread.Run(() =>
        {
            var block = new Block(0, 0, amplitude: 120, width: 24);

            var targets = LayoutCreator.CreateCircular(block, targetCount: 7, fieldSize: 168);

            for (int i = 1; i < targets.Length; i++)
            {
                var previous = targets[i - 1].Position;
                var current = targets[i].Position;
                var distance = Math.Sqrt(
                    Math.Pow(current.X - previous.X, 2) +
                    Math.Pow(current.Y - previous.Y, 2));

                Assert.Equal(block.Amplitude, distance, precision: 6);
            }
        });
    }
}
