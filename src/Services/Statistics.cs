namespace FittsLaw.Services;

internal class Statistics
{
    public IReadOnlyDictionary<string, string[]> Compute(Models.Block[] experimentBlocks)
    {
        int n = experimentBlocks.Length;

        Dictionary<string, string[]> result = [];
        result.Add("Block", new string[n]);
        result.Add("Trials", new string[n]);
        result.Add("Amplitude, px", new string[n]);
        result.Add("Width, px", new string[n]);
        result.Add("Offset, px", new string[n]);
        result.Add("ID, bits", new string[n]);
        result.Add("Eff. Amplitude, px", new string[n]);
        result.Add("Eff. Width, px", new string[n]);
        result.Add("Eff. ID, bits", new string[n]);
        result.Add("MT, ms", new string[n]);
        result.Add("Errors", new string[n]);
        result.Add("Errors, %", new string[n]);
        result.Add("Throughput, b/s", new string[n]);

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

            n = block.Targets.Count() - 1;

            meanDuration /= n;
            effectiveAmplitude /= n;
            meanOffset /= n;
            sd = Math.Sqrt(sd / n);

            var id = Math.Log2(block.Amplitude / block.Width + 1);
            var errors = (double)errorCount / n;
            var throughput = id / (0.001 * meanDuration);  // bits per second

            var effectiveWidth = 4.133 * sd;
            var effectiveId = Math.Log2(effectiveAmplitude / effectiveWidth + 1);

            result["Block"][i] = block.Index.ToString();
            result["Trials"][i] = block.Targets.Count().ToString();
            result["Amplitude, px"][i] = block.Amplitude.ToString();
            result["Width, px"][i] = block.Width.ToString();
            result["Offset, px"][i] = meanOffset.ToString("F2");
            result["ID, bits"][i] = id.ToString("F2");
            result["Eff. Amplitude, px"][i] = effectiveAmplitude.ToString("F2");
            result["Eff. Width, px"][i] = effectiveWidth.ToString("F2");
            result["Eff. ID, bits"][i] = effectiveId.ToString("F2");
            result["MT, ms"][i] = meanDuration.ToString();
            result["Errors"][i] = errorCount.ToString("F1");
            result["Errors, %"][i] = (100.0 * errors).ToString("F1");
            result["Throughput, b/s"][i] = throughput.ToString("F2");

            i++;
        }

        return result;
    }
}
