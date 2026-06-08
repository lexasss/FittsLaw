using System.Windows;

namespace FittsLaw.Helpers;

internal static class PointExtensions
{
#if VS_VERSION_18_0_OR_GREATER
    extension(Point pt)
    {
        public static double Amplitude() => Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
    }
#else
    public static double Amplitude(this Point point) => Math.Sqrt(point.X * point.X + point.Y * point.Y);
#endif
}