using System.Windows.Controls;
using FittsLaw.Helpers;
using FittsLaw.Models;
using TargetViewModel = FittsLaw.ViewModels.Target;

namespace FittsLaw.Tests;

public class BlockUiCreatorTests
{
    [Fact]
    public void CreateBuildsExpectedTargetCountAndSize()
    {
        StaThread.Run(() =>
        {
            var block = new Block(0, amplitude: 120, width: 24);

            var targets = BlockUiCreator.Create(block, targetCount: 7, fieldSize: 168);

            Assert.Equal(7, targets.Length);
            Assert.All(targets, target =>
            {
                Assert.Equal(24, target.Width);
                Assert.Equal(24, target.Height);
                Assert.IsType<TargetViewModel>(target.DataContext);
            });
        });
    }

    [Fact]
    public void CreatePlacesTargetsWithinParentBounds()
    {
        StaThread.Run(() =>
        {
            var block = new Block(0, amplitude: 120, width: 24);
            var fieldSize = block.Amplitude + 2 * block.Width;

            var targets = BlockUiCreator.Create(block, targetCount: 7, fieldSize);

            Assert.All(targets, target =>
            {
                Assert.InRange(Canvas.GetLeft(target), 0, fieldSize - block.Width);
                Assert.InRange(Canvas.GetTop(target), 0, fieldSize - block.Width);
            });
        });
    }

    [Fact]
    public void CreatePlacesConsecutiveTargetsAtBlockAmplitudeDistance()
    {
        StaThread.Run(() =>
        {
            var block = new Block(0, amplitude: 120, width: 24);

            var targets = BlockUiCreator.Create(block, targetCount: 7, fieldSize: 168);
            var viewModels = targets.Select(t => (TargetViewModel)t.DataContext).ToArray();

            for (int i = 1; i < viewModels.Length; i++)
            {
                var previous = viewModels[i - 1].Data.Position;
                var current = viewModels[i].Data.Position;
                var distance = Math.Sqrt(
                    Math.Pow(current.X - previous.X, 2) +
                    Math.Pow(current.Y - previous.Y, 2));

                Assert.Equal(block.Amplitude, distance, precision: 6);
            }
        });
    }
}
