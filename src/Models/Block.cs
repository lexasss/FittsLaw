namespace FittsLaw.Models;

internal class Block(int sessionId, int blockId, double amplitude, double width)
{
    public int SessionId { get; } = sessionId;
    public int BlockId { get; set; } = blockId;
    public double Amplitude { get; } = amplitude;
    public double Width { get; } = width;
    public List<Target> Targets { get; init; } = [];
}
