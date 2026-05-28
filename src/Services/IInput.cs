using System.Windows;

namespace FittsLaw.Services;

internal interface IInput
{
    void Register(Window window, IReadOnlyCollection<object> items, Experiment experiment);
}
