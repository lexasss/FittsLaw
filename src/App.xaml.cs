using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FittsLaw;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    static App()
    {
        ServiceCollection services = new();
        services.AddSingleton<Services.Experiment, Services.Experiment>();

        ServiceProvider = services.BuildServiceProvider();
    }
}
