namespace FittsLaw.Services;

internal class Statistics
{
    public static string[] Fields => [  // the order of fields is important as it is used in the Statistics view
        "Block",
        "Trials",
        "Amplitude, px",
        "Width, px",
        "Offset, px",
        "ID, bits",
        "Eff. Amplitude, px",
        "Eff. Width, px",
        "Eff. ID, bits",
        "MT, ms",
        "Errors",
        "Errors, %",
        "Throughput, b/s",
        "Eff. Throughput, b/s"
    ];

    public IReadOnlyDictionary<string, string[]> Compute(Models.Block[] experimentBlocks)
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
            double startX = 0, startY = 0;

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
                    dx = activationX - startX;
                    dy = activationY - startY;
                    effectiveAmplitude += Math.Sqrt(dx * dx + dy * dy);

                    meanDuration += target.ActivationTimestamp - startTimestamp;

                    validTrialCount += 1;
                }

                startTimestamp = target.ActivationTimestamp;
                startX = activationX;
                startY = activationY;
            }

            result[Fields[0]][i] = (i + 1).ToString();
            result[Fields[1]][i] = validTrialCount.ToString();
            result[Fields[2]][i] = block.Amplitude.ToString();
            result[Fields[3]][i] = block.Width.ToString();

            if (validTrialCount == 0)
            {   // the computed values are set to 0 indicating the block is invalid for analysis
                for (int j = 4; j < Fields.Length; j++)
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

            result[Fields[4]][i] = meanOffset.ToString("F1");
            result[Fields[5]][i] = id.ToString("F2");
            result[Fields[6]][i] = effectiveAmplitude.ToString("F1");
            result[Fields[7]][i] = effectiveWidth.ToString("F1");
            result[Fields[8]][i] = effectiveId.ToString("F2");
            result[Fields[9]][i] = meanDuration.ToString();
            result[Fields[10]][i] = errorCount.ToString();
            result[Fields[11]][i] = (100.0 * errors).ToString("F1");
            result[Fields[12]][i] = throughput.ToString("F2");
            result[Fields[13]][i] = effectiveThroughput.ToString("F2");
        }

        return result;
    }

    #region Internal

    const string ZERO = "0";

    #endregion
}
