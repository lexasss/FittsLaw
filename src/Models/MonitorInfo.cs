namespace FittsLaw.Models;

internal class MonitorInfo
{
    public string SerialNumberID { get; set; } = string.Empty;
    public string DeviceID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string FrendlyName { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Name} ({Manufacturer})";
    }
}
