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
        services.AddSingleton<Services.Statistics, Services.Statistics>();
        services.AddSingleton<Services.MouseInput>();
        services.AddSingleton<Services.TouchInput>();

        services.AddSingleton<Func<string, Services.IInput>>(sp => key =>
        {
            return key switch
            {
                "mouse" => sp.GetRequiredService<Services.MouseInput>(),
                "touch" => sp.GetRequiredService<Services.TouchInput>(),
                _ => throw new ArgumentException()
            };
        });

        ServiceProvider = services.BuildServiceProvider();
    }
}
