using System.Windows.Controls;

namespace FittsLaw.Helpers;

internal static class BlockUiCreator
{
    /// <summary>
    /// Creates an array of targets for the given block, arranged in a circle.
    /// The distance between subsequent targets on the circle equals to the amplitude of the block,
    /// and the size of each target equals to the width of the block.
    /// </summary>
    /// <param name="block">Block parametgers</param>
    /// <param name="targetCount">number of target</param>
    /// <param name="fieldSize">size of the parent</param>
    /// <returns></returns>
    public static Views.Target[] Create(Models.Block block, int targetCount, double fieldSize)
    {
        var center = fieldSize / 2.0;
        var angle = 2.0 * Math.PI / targetCount;    // between two adjacent targets on the circle
        var radius = GetCircleRadius(targetCount, block.Amplitude) + block.Width / 2;

        var targets = new Views.Target[targetCount];
        int halfTargetCount = targetCount / 2 + 1;
        int angleIndex = 0;

        for (int i = 0; i < targetCount; i++)
        {
            var target = new Views.Target
            {
                Width = block.Width,
                Height = block.Width
            };

            var x = center + radius * Math.Cos(angleIndex * angle);
            var y = center + radius * Math.Sin(angleIndex * angle);

            angleIndex = (angleIndex + halfTargetCount) % targetCount;

            Canvas.SetLeft(target, x - block.Width / 2);
            Canvas.SetTop(target, y - block.Width / 2);

            targets[i] = target;

            var data = (target.DataContext as ViewModels.Target)!.Data;
            data.Id = i;
            data.Size = block.Width;
            data.Position = new System.Windows.Point(x, y);
        }

        return targets;
    }

    #region Internal

    private static double GetCircleRadius(int targetCount, double amplitude)
    {
        // Radius of the circle will always be greater than half the amplitude:
        //                  A
        // R = ---------------------------
        //     sqrt(2 * (1 + cos(180°/N)))
        //
        // This ensures that the distance between targets activate on the circle subsequently equals to A

        double doubleAngle = Math.PI / targetCount;
        return amplitude / Math.Sqrt(2.0 * (1.0 + Math.Cos(doubleAngle)));
    }

    #endregion
}
