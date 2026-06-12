using CommunityToolkit.Mvvm.ComponentModel;

namespace FittsLaw.Models;

internal partial class Size : ObservableObject
{
    [ObservableProperty]
    public partial int Height { get; set; }
    [ObservableProperty]
    public partial int Width { get; set; }

    public static Size Default { get; } = new Size();
}
