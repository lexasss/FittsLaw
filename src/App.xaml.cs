using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FittsLaw;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
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

        services.AddTransient<ViewModels.Main>();
        services.AddTransient<ViewModels.Experiment>();
        services.AddTransient<ViewModels.MainWindow>();
        services.AddTransient<ViewModels.Setup>();
        services.AddTransient<ViewModels.Statistics>();

        services.AddTransient<Views.Main>(sp => new Views.Main(
            sp.GetRequiredService<ViewModels.Main>()));
        services.AddTransient<Views.Experiment>(sp => new Views.Experiment(
            sp.GetRequiredService<ViewModels.Experiment>()));
        services.AddTransient<Views.MainWindow>(sp => new Views.MainWindow(
            sp.GetRequiredService<ViewModels.MainWindow>()));
        services.AddTransient<Views.Setup>(sp => new Views.Setup(
            sp.GetRequiredService<ViewModels.Setup>()));
        services.AddTransient<Views.Statistics>(sp => new Views.Statistics(
            sp.GetRequiredService<ViewModels.Statistics>()));

        services.AddTransient<Func<Views.Setup>>(sp =>
            () => sp.GetRequiredService<Views.Setup>());
        services.AddTransient<Func<Views.Experiment>>(sp =>
            () => sp.GetRequiredService<Views.Experiment>());
        services.AddTransient<Func<IReadOnlyDictionary<string, string[]>, Views.Statistics>>(sp =>
            statisticsData =>
            {
                var dialog = sp.GetRequiredService<Views.Statistics>();
                dialog.SetStatisticsData(statisticsData);
                return dialog;
            });

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<Views.MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}
