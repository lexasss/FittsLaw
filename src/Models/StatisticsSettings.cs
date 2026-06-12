namespace FittsLaw.Models;

internal record class StatisticsSettings(double CriticalErrorRate)
{
    public static StatisticsSettings From(Properties.Settings settings)
    {
        return new StatisticsSettings(settings.CriticalErrorRate);
    }

    public void Save()
    {
        var props = Properties.Settings.Default;
        props.CriticalErrorRate = CriticalErrorRate;

        props.Save();
    }
}
