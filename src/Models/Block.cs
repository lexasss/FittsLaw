namespace FittsLaw.Models;

internal class Block(int index, double amplitude, double width)
{
    public int Index { get; } = index;
    public double Amplitude { get; } = amplitude;
    public double Width { get; } = width;
    public IEnumerable<Target> Targets { get; set; } = [];
}
