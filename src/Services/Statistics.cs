using System.Windows;
using FittsLaw.Extensions;

namespace FittsLaw.Services;

internal class Statistics
{
    public static string[] Fields { get; } = StatFields.Select(sf => sf.Name).ToArray();
    public static int FirstComputedFieldIndex { get; } = 4;

    public static IReadOnlyDictionary<string, string[]> Compute(Models.Block[] experimentBlocks)
    {
        var settings = Models.StatisticsSettings.From(Properties.Settings.Default);

        if (experimentBlocks[0].Amplitude == 0)
        {
            return ComputeForVaryingAmplitude(experimentBlocks, settings);
        }
        else
        {
            return ComputeForFixedAmplitude(experimentBlocks, settings);
        }
    }

    #region Internal

    record class FieldInfo(string Name, string Format);

    const string ZERO = "0";

    static FieldInfo[] StatFields => [  // the order of fields is important as it is used in the Statistics view
        // conditions
        new FieldInfo("Block", "F0"),
        new FieldInfo("Trials", "F0"),
        new FieldInfo("Amplitude, px", "F0"),
        new FieldInfo("Width, px", "F0"),

        // computed data
        new FieldInfo("Offset, px", "F1"),
        new FieldInfo("ID, bits", "F2"),
        new FieldInfo("Eff. Amplitude, px", "F1"),
        new FieldInfo("Eff. Width, px", "F1"),
        new FieldInfo("Eff. ID, bits", "F2"),
        new FieldInfo("MT, ms", "F0"),
        new FieldInfo("Errors", "F0"),
        new FieldInfo("Errors, %", "F1"),
        new FieldInfo("Throughput, b/s", "F2"),
        new FieldInfo("Eff. Throughput, b/s", "F2")
    ];

    static string[] Formats { get; } = StatFields.Select(sf => sf.Format).ToArray();

    public static IReadOnlyDictionary<string, string[]> ComputeForVaryingAmplitude(
        Models.Block[] experimentBlocks,
        Models.StatisticsSettings settings)
    {
        // Because of varying amplitude, statistics is computed in few steps

        var newBlocks = new List<Models.Block>();

        // First, create a list of blocks so that each block gets two trials, the first one simply to be a reference,
        // while only the second trial matters and will appear in statistics

        foreach (var block in experimentBlocks)
        {
            Models.Target? prevTarget = null;
            foreach (var target in block.Targets)
            {
                if (prevTarget != null)
                {
                    var amplitude = Distance(target.Position, prevTarget.Position);
                    newBlocks.Add(new Models.Block(0, amplitude, block.Width)
                    {
                        Targets = [prevTarget, target]
                    });
                }

                prevTarget = target;
            }
        }

        // Then, compute statistics for each trial separately
        var targetStats = ComputeForFixedAmplitude(
            newBlocks.ToArray(),
            new Models.StatisticsSettings(1)); // do not filter errors yet

        // Next, unite trials with the same amplitude and width into their own blocks

        var unitedStats = new Dictionary<int, double[]>();

        var amplitudes = targetStats[Fields[2]];
        var widths = targetStats[Fields[3]];

        for (int i = 0; i < amplitudes.Length; i++)
        {
            var amplitude = double.Parse(amplitudes[i]);
            var width = double.Parse(widths[i]);

            var hash = HashCode.Combine(amplitude, width);
            if (unitedStats.TryGetValue(hash, out var unitedStat))
            {
                unitedStat[1]++;    // trial count
                for (int j = FirstComputedFieldIndex; j < Fields.Length; j++)
                    unitedStat[j] += double.Parse(targetStats[Fields[j]][i]);
            }
            else
            {
                unitedStats.Add(hash, [
                    0,
                    1,
                    amplitude,
                    width,
                    double.Parse(targetStats[Fields[4]][i]),
                    double.Parse(targetStats[Fields[5]][i]),
                    double.Parse(targetStats[Fields[6]][i]),
                    double.Parse(targetStats[Fields[7]][i]),
                    double.Parse(targetStats[Fields[8]][i]),
                    double.Parse(targetStats[Fields[9]][i]),
                    double.Parse(targetStats[Fields[10]][i]),
                    double.Parse(targetStats[Fields[11]][i]),
                    double.Parse(targetStats[Fields[12]][i]),
                    double.Parse(targetStats[Fields[13]][i])
                ]);
            }
        }

        var blocks = unitedStats.Values.ToArray();
        foreach (var block in blocks)
        {
            for (int j = FirstComputedFieldIndex; j < block.Length; j++)
            {
                if (j != 10)    // errors remain a sum of errors, not the average
                    block[j] /= block[1];
            }
        }

        // Sort by Widths, then by Amplitudes
        blocks.Sort((a, b) => (a[3] != b[3] ? a[3] > b[3] : a[2] > b[2]) ? 1 : -1);

        // Finally, create statitics rows..

        Dictionary<string, string[]> statRows = [];
        foreach (var field in Fields)
            statRows.Add(field, new string[blocks.Length]);

        for (int i = 0; i < blocks.Length; i++)
        {
            var block = blocks[i];
            for (int j = 0; j < StatFields.Length; j++)
            {
                statRows[Fields[j]][i] = block[j].ToString(Formats[j]);
            }
        }

        // .. and filter too-many-errors blocks

        var errorPercentageThreshold = settings.CriticalErrorRate * 100;
        var errorPercentages = statRows[Fields[11]].Select(double.Parse).ToArray();

        Dictionary<string, string[]> result = [];
        foreach (var kv in statRows)
        {
            result[kv.Key] = kv.Value.Where((_, j) => errorPercentages[j] <= errorPercentageThreshold).ToArray();
        }
        return result;
    }

    public static IReadOnlyDictionary<string, string[]> ComputeForFixedAmplitude(
        Models.Block[] experimentBlocks,
        Models.StatisticsSettings settings)
    {
        int blockCount = experimentBlocks.Length;

        Dictionary<string, string[]> statRows = [];
        foreach (var field in Fields)
            statRows.Add(field, new string[blockCount]);

        experimentBlocks.Sort((b1, b2) => b1.Id.CompareTo(b2.Id));

        for (int i = 0; i < experimentBlocks.Length; i++)
        {
            var block = experimentBlocks[i];

            long meanDuration = 0;
            double effectiveAmplitude = 0;
            double meanOffset = 0;
            double sd = 0;
            int errorCount = 0;

            long startTimestamp = 0;
            Point prevActivation = new(0, 0);
            Point prevTarget = new(0, 0);

            int validTrialCount = 0;
            foreach (var target in block.Targets)
            {
                Point activation = target.Position.Add(target.ActivationOffset);

                if (startTimestamp > 0)
                {
                    double offset = target.ActivationOffset.Amplitude();
                    bool isError = offset > block.Width / 2;
                    errorCount += isError ? 1 : 0;

                    meanOffset += offset;
                    sd += offset * offset;

                    effectiveAmplitude += GetEffectiveAmplitude(
                        //in prevTarget,
                        //target.Position,
                        prevActivation,
                        activation);

                    meanDuration += target.ActivationTimestamp - startTimestamp;

                    validTrialCount += 1;
                }

                startTimestamp = target.ActivationTimestamp;
                prevActivation = activation;
                prevTarget = target.Position;
            }

            statRows[Fields[0]][i] = (i + 1).ToString(Formats[0]);
            statRows[Fields[1]][i] = validTrialCount.ToString(Formats[1]);
            statRows[Fields[2]][i] = block.Amplitude.ToString(Formats[2]);
            statRows[Fields[3]][i] = block.Width.ToString(Formats[3]);

            if (validTrialCount < 2)        // at least 2 trials must be valid
            {   // the computed values are set to 0 indicating the block is invalid for analysis
                for (int j = FirstComputedFieldIndex; j < Fields.Length; j++)
                    statRows[Fields[j]][i] = ZERO;
                continue;
            }

            meanDuration /= validTrialCount;
            effectiveAmplitude /= validTrialCount;
            meanOffset /= validTrialCount;
            sd = Math.Sqrt(sd / (validTrialCount - 1)); // -1, as this is a SAMPLE, not POPULATION

            double id = Math.Log2(block.Amplitude / block.Width + 1);
            double errors = (double)errorCount / validTrialCount;
            double throughput = id / (0.001 * meanDuration);  // bits per second

            double effectiveWidth = 4.133 * sd;
            double effectiveId = effectiveWidth > 0 ? Math.Log2(effectiveAmplitude / effectiveWidth + 1) : 0;
            double effectiveThroughput = effectiveId / (0.001 * meanDuration);  // bits per second

            statRows[Fields[4]][i] = meanOffset.ToString(Formats[4]);
            statRows[Fields[5]][i] = id.ToString(Formats[5]);
            statRows[Fields[6]][i] = effectiveAmplitude.ToString(Formats[6]);
            statRows[Fields[7]][i] = effectiveWidth.ToString(Formats[7]);
            statRows[Fields[8]][i] = effectiveId.ToString(Formats[8]);
            statRows[Fields[9]][i] = meanDuration.ToString(Formats[9]);
            statRows[Fields[10]][i] = errorCount.ToString(Formats[10]);
            statRows[Fields[11]][i] = (100.0 * errors).ToString(Formats[11]);
            statRows[Fields[12]][i] = throughput.ToString(Formats[12]);
            statRows[Fields[13]][i] = effectiveThroughput.ToString(Formats[13]);
        }

        // Filtering

        var errorPercentageThreshold = settings.CriticalErrorRate * 100;
        var errorPercentages = statRows[Fields[11]].Select(double.Parse).ToArray();

        Dictionary<string, string[]> result = [];
        foreach (var kv in statRows)
        {
            result[kv.Key] = kv.Value.Where((_, j) => errorPercentages[j] <= errorPercentageThreshold).ToArray();
        }

        return result;
    }

    #endregion

    #region Internal

    // based on https://www.yorku.ca/mack/FittsLawSoftware/doc/Throughput.html
    private static double GetEffectiveAmplitude(
        in Point from,
        in Point to,
        in Point activation)
    {
        double a = Distance(from, to);
        double b = Distance(to, activation);
        double c = Distance(from, activation);

        double dx = (c * c - b * b - a * a) / (2.0 * a);
        return a + dx;
    }

    // based on https://www.yorku.ca/mack/ijhcs2004.pdf
    private static double GetEffectiveAmplitude(
        in Point previousActivation,
        in Point activation) =>
        Distance(previousActivation, activation);

    private static double Distance(
        in Point p1,
        in Point p2)
    {
        double dx = p1.X - p2.X;
        double dy = p1.Y - p2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    #endregion
}
