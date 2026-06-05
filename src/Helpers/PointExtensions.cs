using System.Windows;

namespace FittsLaw.Helpers;

internal static class PointExtensions
{
    public static double Amplitude(this Point point) => Math.Sqrt(point.X * point.X + point.Y * point.Y);
}