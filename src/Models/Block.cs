namespace FittsLaw.Models;

internal class Block(int id, double amplitude, double width)
{
    public int Id { get; } = id;
    public double Amplitude { get; } = amplitude;
    public double Width { get; } = width;
    public IEnumerable<Target> Targets { get; set; } = [];
}
