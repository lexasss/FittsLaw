namespace FittsLaw.Services;

internal class Statistics
{
    public static string[] Fields => [
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
        "Throughput, b/s"
    ];

    public IReadOnlyDictionary<string, string[]> Compute(Models.Block[] experimentBlocks)
    {
        int n = experimentBlocks.Length;

        Dictionary<string, string[]> result = [];
        result.Add(Fields[0], new string[n]);
        result.Add(Fields[1], new string[n]);
        result.Add(Fields[2], new string[n]);
        result.Add(Fields[3], new string[n]);
        result.Add(Fields[4], new string[n]);
        result.Add(Fields[5], new string[n]);
        result.Add(Fields[6], new string[n]);
        result.Add(Fields[7], new string[n]);
        result.Add(Fields[8], new string[n]);
        result.Add(Fields[9], new string[n]);
        result.Add(Fields[10], new string[n]);
        result.Add(Fields[11], new string[n]);
        result.Add(Fields[12], new string[n]);

        experimentBlocks.Sort((b1, b2) => b1.Index.CompareTo(b2.Index));

        int i = 0;
        foreach (var block in experimentBlocks)
        {
            long meanDuration = 0;
            double effectiveAmplitude = 0;
            double meanOffset = 0;
            double sd = 0;
            int errorCount = 0;

            long startTimestamp = 0;
            double startX = 0, startY = 0;

            foreach (var target in block.Targets)
            {
                var activationX = target.Position.X + target.ActivationOffset.X;
                var activationY = target.Position.Y + target.ActivationOffset.Y;

                if (startTimestamp > 0)
                {
                    meanDuration += target.ActivationTimestamp - startTimestamp;

                    var dx = target.ActivationOffset.X;
                    var dy = target.ActivationOffset.Y;
                    var offset = Math.Sqrt(dx * dx + dy * dy);
                    meanOffset += offset;

                    sd += offset * offset;
                    dx = activationX - startX;
                    dy = activationY - startY;
                    effectiveAmplitude += Math.Sqrt(dx * dx + dy * dy);

                    errorCount += offset > block.Width / 2 ? 1 : 0;
                }

                startTimestamp = target.ActivationTimestamp;
                startX = activationX;
                startY = activationY;
            }

            result[Fields[0]][i] = (i + 1).ToString();
            result[Fields[1]][i] = block.Targets.Count().ToString();
            result[Fields[2]][i] = block.Amplitude.ToString();
            result[Fields[3]][i] = block.Width.ToString();

            n = block.Targets.Count() - 1;

            if (n > 0)
            {
                meanDuration /= n;
                effectiveAmplitude /= n;
                meanOffset /= n;
                sd = Math.Sqrt(sd / n);

                var id = Math.Log2(block.Amplitude / block.Width + 1);
                var errors = (double)errorCount / n;
                var throughput = id / (0.001 * meanDuration);  // bits per second

                var effectiveWidth = 4.133 * sd;
                var effectiveId = effectiveWidth > 0 ? Math.Log2(effectiveAmplitude / effectiveWidth + 1) : 0;

                result[Fields[4]][i] = meanOffset.ToString("F1");
                result[Fields[5]][i] = id.ToString("F2");
                result[Fields[6]][i] = effectiveAmplitude.ToString("F1");
                result[Fields[7]][i] = effectiveWidth.ToString("F1");
                result[Fields[8]][i] = effectiveId.ToString("F2");
                result[Fields[9]][i] = meanDuration.ToString();
                result[Fields[10]][i] = errorCount.ToString();
                result[Fields[11]][i] = (100.0 * errors).ToString("F1");
                result[Fields[12]][i] = throughput.ToString("F2");
            }
            else // otherwise the computed values are set to 0 indicating the block is invalid for analysis
            {
                result[Fields[4]][i] = Zero;
                result[Fields[5]][i] = Zero;
                result[Fields[6]][i] = Zero;
                result[Fields[7]][i] = Zero;
                result[Fields[8]][i] = Zero;
                result[Fields[9]][i] = Zero;
                result[Fields[10]][i] = Zero;
                result[Fields[11]][i] = Zero;
                result[Fields[12]][i] = Zero;
            }

            i++;
        }

        return result;
    }

    readonly string Zero = "0";
}
