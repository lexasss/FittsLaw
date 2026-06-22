using System.Windows;

namespace FittsLaw.Extensions;

internal static class PointExtensions
{
#if VS_VERSION_18_0_OR_GREATER
    extension(Point point)
    {
        public double Amplitude() =>
        Math.Sqrt(point.X * point.X + point.Y * point.Y);

        public Point Add(in Point p) =>
            new(point.X + p.X, point.Y + p.Y);
    }
#else
    public static double Amplitude(this Point point) =>
        Math.Sqrt(point.X * point.X + point.Y * point.Y);

    public static Point Add(this Point point, in Point p) =>
        new(point.X + p.X, point.Y + p.Y);

    public static double DistanceTo(this Point point, in Point p)
    {
        double dx = p.X - point.X;
        double dy = p.Y - point.Y;
        return Math.Sqrt(dx * dx + dy *dy);
    }
#endif
}