using System.Windows;
using FittsLaw.Extensions;

namespace FittsLaw.Services;

internal class Statistics
{
    public enum TargetsToSkip
    {
        First,
        Odds,
    }

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
            return ComputeForFixedAmplitude(experimentBlocks, settings, TargetsToSkip.First);
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
                    var amplitude = target.Position.DistanceTo(prevTarget.Position);
                    var newBlock = new Models.Block(0, amplitude, block.Width);
                    newBlock.Targets.Add(prevTarget);
                    newBlock.Targets.Add(target);
                    newBlocks.Add(newBlock);
                }

                prevTarget = target;
            }
        }

        // Unite blocks of the same amplitude and width
        var unitedBlocks = new Dictionary<int, Models.Block>();
        foreach (var block in newBlocks)
        {
            var hash = HashCode.Combine(block.Amplitude, block.Width);
            if (unitedBlocks.TryGetValue(hash, out var unitedBlock))
            {
                unitedBlock.Targets.AddRange(block.Targets);
            }
            else
            {
                unitedBlocks.Add(hash, block);
            }
        }

        // Sort according width and amplitude
        var blocks = unitedBlocks.Values.ToArray();
        blocks.Sort((a, b) => {
            double d = a.Width - b.Width;
            if (d == 0)
                d = a.Amplitude - b.Amplitude;
            return Math.Sign(d);
        });

        // Finally compute statistics with a special flag to skip every odd target, not just the first one
        var statRows = ComputeForFixedAmplitude(
            blocks,
            settings,
            TargetsToSkip.Odds);

        return statRows;
    }

    public static IReadOnlyDictionary<string, string[]> ComputeForFixedAmplitude(
        Models.Block[] experimentBlocks,
        Models.StatisticsSettings settings,
        TargetsToSkip targetsToSkip)
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

                    startTimestamp = targetsToSkip switch
                    {
                        TargetsToSkip.Odds => 0,
                        TargetsToSkip.First => target.ActivationTimestamp,
                        _ => throw new NotImplementedException()
                    };
                }
                else
                {
                    startTimestamp = target.ActivationTimestamp;
                }

                prevActivation = activation;
                prevTarget = target.Position;
            }

            statRows[Fields[0]][i] = (i + 1).ToString(Formats[0]);
            statRows[Fields[1]][i] = validTrialCount.ToString(Formats[1]);
            statRows[Fields[2]][i] = block.Amplitude.ToString(Formats[2]);
            statRows[Fields[3]][i] = block.Width.ToString(Formats[3]);

            if (validTrialCount < 1)        // at least 1 trial must be valid
            {   // the computed values are set to 0 indicating the block is invalid for analysis
                for (int j = FirstComputedFieldIndex; j < Fields.Length; j++)
                    statRows[Fields[j]][i] = ZERO;
                continue;
            }

            meanDuration /= validTrialCount;
            effectiveAmplitude /= validTrialCount;
            meanOffset /= validTrialCount;
            sd = Math.Sqrt(sd / validTrialCount); // it used to be validTrialCount - 1, as this is a SAMPLE, not POPULATION, but rejected due to grid

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
        double a = from.DistanceTo(to);
        double b = to.DistanceTo(activation);
        double c = from.DistanceTo(activation);

        double dx = (c * c - b * b - a * a) / (2.0 * a);
        return a + dx;
    }

    // based on https://www.yorku.ca/mack/ijhcs2004.pdf
    private static double GetEffectiveAmplitude(
        in Point previousActivation,
        in Point activation) =>
        activation.DistanceTo(previousActivation);

    #endregion
}
