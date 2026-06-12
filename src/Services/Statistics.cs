namespace FittsLaw.Services;

internal class Statistics
{
    public static string[] Fields { get; } = StatFields.Select(sf => sf.Name).ToArray();
    public static int FirstComputedFieldIndex { get; } = 4;

    public IReadOnlyDictionary<string, string[]> Compute(Models.Block[] experimentBlocks)
    {
        if (experimentBlocks[0].Amplitude == 0)
        {
            return ComputeForVaryingAmplitude(experimentBlocks);
        }
        else
        {
            return ComputeForFixedAmplitude(experimentBlocks);
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

    public static IReadOnlyDictionary<string, string[]> ComputeForVaryingAmplitude(Models.Block[] experimentBlocks)
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
                    var dx = target.Position.X - prevTarget.Position.X;
                    var dy = target.Position.Y - prevTarget.Position.Y;
                    var amplitude = Math.Sqrt(dx * dx + dy * dy);

                    newBlocks.Add(new Models.Block(0, amplitude, block.Width)
                    {
                        Targets = [prevTarget, target]
                    });
                }

                prevTarget = target;
            }
        }

        // Then, compute statistics for each trial separately
        var targetStats = ComputeForFixedAmplitude(newBlocks.ToArray());

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

        blocks.Sort((a, b) => (a[3] != b[3] ? a[3] > b[3] : a[2] > b[2]) ? 1 : -1);

        // Finally, create 

        Dictionary<string, string[]> result = [];
        foreach (var field in Fields)
            result.Add(field, new string[blocks.Length]);

        for (int i = 0; i < blocks.Length; i++)
        {
            var block = blocks[i];
            for (int j = 0; j < StatFields.Length; j++)
            {
                result[Fields[j]][i] = block[j].ToString(Formats[j]);
            }
        }

        return result;
    }

    public static IReadOnlyDictionary<string, string[]> ComputeForFixedAmplitude(Models.Block[] experimentBlocks)
    {
        int blockCount = experimentBlocks.Length;

        Dictionary<string, string[]> result = [];
        foreach (var field in Fields)
            result.Add(field, new string[blockCount]);

        experimentBlocks.Sort((b1, b2) => b1.Index.CompareTo(b2.Index));

        for (int i = 0; i < experimentBlocks.Length; i++)
        {
            var block = experimentBlocks[i];

            long meanDuration = 0;
            double effectiveAmplitude = 0;
            double meanOffset = 0;
            double sd = 0;
            int errorCount = 0;

            long startTimestamp = 0;
            double prevActivationX = 0,
                   prevActivationY = 0;
            double prevTargetX = 0,
                   prevTargetY = 0;

            int validTrialCount = 0;
            foreach (var target in block.Targets)
            {
                double activationX = target.Position.X + target.ActivationOffset.X;
                double activationY = target.Position.Y + target.ActivationOffset.Y;

                if (startTimestamp > 0)
                {
                    double dx = target.ActivationOffset.X;
                    double dy = target.ActivationOffset.Y;
                    double offset = Math.Sqrt(dx * dx + dy * dy);
                    bool isError = offset > block.Width / 2;
                    errorCount += isError ? 1 : 0;

                    meanOffset += offset;

                    sd += offset * offset;
                    dx = activationX - prevActivationX;
                    dy = activationY - prevActivationY;
                    effectiveAmplitude += Math.Sqrt(dx * dx + dy * dy);

                    meanDuration += target.ActivationTimestamp - startTimestamp;

                    validTrialCount += 1;
                }

                startTimestamp = target.ActivationTimestamp;
                prevActivationX = activationX;
                prevActivationY = activationY;
                prevTargetX = target.Position.X;
                prevTargetY = target.Position.Y;
            }

            result[Fields[0]][i] = (i + 1).ToString(Formats[0]);
            result[Fields[1]][i] = validTrialCount.ToString(Formats[1]);
            result[Fields[2]][i] = block.Amplitude.ToString(Formats[2]);
            result[Fields[3]][i] = block.Width.ToString(Formats[3]);

            if (validTrialCount == 0)
            {   // the computed values are set to 0 indicating the block is invalid for analysis
                for (int j = FirstComputedFieldIndex; j < Fields.Length; j++)
                    result[Fields[j]][i] = ZERO;
                continue;
            }

            meanDuration /= validTrialCount;
            effectiveAmplitude /= validTrialCount;
            meanOffset /= validTrialCount;
            sd = Math.Sqrt(sd / validTrialCount);

            double id = Math.Log2(block.Amplitude / block.Width + 1);
            double errors = (double)errorCount / validTrialCount;
            double throughput = id / (0.001 * meanDuration);  // bits per second

            double effectiveWidth = 4.133 * sd;
            double effectiveId = effectiveWidth > 0 ? Math.Log2(effectiveAmplitude / effectiveWidth + 1) : 0;
            double effectiveThroughput = effectiveId / (0.001 * meanDuration);  // bits per second

            result[Fields[4]][i] = meanOffset.ToString(Formats[4]);
            result[Fields[5]][i] = id.ToString(Formats[5]);
            result[Fields[6]][i] = effectiveAmplitude.ToString(Formats[6]);
            result[Fields[7]][i] = effectiveWidth.ToString(Formats[7]);
            result[Fields[8]][i] = effectiveId.ToString(Formats[8]);
            result[Fields[9]][i] = meanDuration.ToString(Formats[9]);
            result[Fields[10]][i] = errorCount.ToString(Formats[10]);
            result[Fields[11]][i] = (100.0 * errors).ToString(Formats[11]);
            result[Fields[12]][i] = throughput.ToString(Formats[12]);
            result[Fields[13]][i] = effectiveThroughput.ToString(Formats[13]);
        }

        return result;
    }

    #endregion
}
