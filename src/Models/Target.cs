using System.Windows;

namespace FittsLaw.Models;

internal class Target
{
    public int Id { get; set; }
    public Point Position { get; set; }
    public double Size { get; set; }
    public bool IsActive { get; set; } = false;
    public Point ActivationLocation { get; set; }
    public long ActivationTimestamp { get; set; }
}