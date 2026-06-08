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

        services.AddSingleton<Func<string, Services.IInput>>(sp => key =>
        {
            string className = key.Replace(" ", string.Empty) + "Input";
            var serviceType = typeof(Services.IInput)
                .Assembly
                .GetTypes()
                .FirstOrDefault(type =>
                    type.Namespace == typeof(Services.IInput).Namespace &&
                    typeof(Services.IInput).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    type.IsClass &&
                    type.Name == className);

            if (serviceType != null)
                return (Services.IInput)ActivatorUtilities.GetServiceOrCreateInstance(sp, serviceType);

            throw new ArgumentException($"Unknown input type: {key}");
        });

        ServiceProvider = services.BuildServiceProvider();
    }
}
