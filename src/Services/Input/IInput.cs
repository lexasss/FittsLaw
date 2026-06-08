using System.Windows;

namespace FittsLaw.Services;

internal interface IInput
{
    void Register(Experiment experiment, UIElement root, UIElement container, Func<IEnumerable<Models.Target>> targetProvider);
}
