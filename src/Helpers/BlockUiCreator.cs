using System.Windows.Controls;
using System.Windows.Shapes;

namespace FittsLaw.Helpers;

internal static class BlockUiCreator
{
    public static Ellipse[] Create(Models.Block block, int targetCount, int fieldSize)
    {
        var uiSettings = Models.UiSettings.From(Properties.Settings.Default);

        var center = fieldSize / 2.0;
        var angle = 360.0 / targetCount;

        var targets = new Ellipse[targetCount];
        for (int i = 0; i < targetCount; i++)
        {
            var ellipse = new Ellipse
            {
                Width = block.Width,
                Height = block.Width,
                Stroke = uiSettings.Foreground,
                StrokeThickness = 1
            };

            var x = center + (block.Amplitude / 2.0 + block.Width / 2.0) * Math.Cos(i * angle * Math.PI / 180);
            var y = center + (block.Amplitude / 2.0 + block.Width / 2.0) * Math.Sin(i * angle * Math.PI / 180);

            Canvas.SetLeft(ellipse, x - block.Width / 2);
            Canvas.SetTop(ellipse, y - block.Width / 2);

            targets[i] = ellipse;
        }

        return targets;
    }
}
