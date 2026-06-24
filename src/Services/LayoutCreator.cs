namespace FittsLaw.Services;

internal enum LayoutType
{
    Circular,
    Grid
}

internal static class LayoutCreator
{
    /// <summary>
    /// Creates an array of targets for the given block, arranged in a circle.
    /// The distance between subsequent targets on the circle equals to the amplitude of the block,
    /// and the size of each target equals to the width of the block.
    /// </summary>
    /// <param name="block">block parameters</param>
    /// <param name="targetCount">number of targets (circles)</param>
    /// <param name="fieldSize">size of the container</param>
    /// <returns>List of targets</returns>
    public static Models.Target[] CreateCircular(
        Models.Block block,
        int targetCount,
        double fieldSize)
    {
        var center = fieldSize / 2.0;
        var angle = 2.0 * Math.PI / targetCount;    // between two adjacent targets on the circle
        var radius = GetCircleRadius(targetCount, block.Amplitude);

        var targets = new Models.Target[targetCount];

        int halfTargetCount = targetCount / 2 + 1;
        int angleIndex = 0;

        for (int i = 0; i < targetCount; i++)
        {
            var x = center + radius * Math.Cos(angleIndex * angle);
            var y = center + radius * Math.Sin(angleIndex * angle);

            var target = new Models.Target()
            {
                Id = i,
                Size = block.Width,
                Position = new System.Windows.Point(x, y)
            };

            angleIndex = (angleIndex + halfTargetCount) % targetCount;

            targets[i] = target;
        }

        return targets;
    }
    /// <summary>
    /// Creates an array of targets arranged on a grid.
    /// The targets appear in the middle of each grid cell.
    /// The size of each target equals to the width of the block.
    /// </summary>
    /// <param name="block">block parameters</param>
    /// <param name="targetCount">number of targets (circles)</param>
    /// <param name="fieldSize">size of the container</param>
    /// <returns>List of targets</returns>
    public static Models.Target[] CreateGrid(
        Models.Block block,
        Models.Size gridSize,
        System.Windows.Size fieldSize)
    {
        var targets = new Models.Target[gridSize.Height * gridSize.Width];

        double cellWidth = fieldSize.Width / gridSize.Width;
        double cellHeight = fieldSize.Height / gridSize.Height;
        double cellCenterX = cellWidth / 2;
        double cellCenterY = cellHeight / 2;

        for (int row = 0; row < gridSize.Height; row++)
        {
            for (int col = 0; col < gridSize.Width; col++)
            {
                int i = row * gridSize.Width + col;
                double x = cellWidth * col + cellCenterX;
                double y = cellHeight * row + cellCenterY;
                var target = new Models.Target()
                {
                    Id = i,
                    Size = block.Width,
                    Position = new System.Windows.Point(x, y)
                };
                targets[i] = target;
            }

        }

        Random.Shared.Shuffle(targets);

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
