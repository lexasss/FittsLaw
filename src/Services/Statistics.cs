namespace FittsLaw.Services;

internal class Statistics
{
    public Models.StatisticsData[] Compute(Models.Block[] experimentBlocks)
    {
        var result = new List<Models.StatisticsData>();

        experimentBlocks.Sort((b1, b2) => b1.Index.CompareTo(b2.Index));

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

            var n = block.Targets.Count() - 1;

            meanDuration /= n;
            effectiveAmplitude /= n;
            meanOffset /= n;
            sd = Math.Sqrt(sd / n);

            var id = Math.Log2(block.Amplitude / block.Width + 1);
            var errors = (double)errorCount / n;
            var throughput = id / (0.001 * meanDuration);  // bits per second

            var effectiveWidth = 4.133 * sd;
            var effectiveId = Math.Log2(effectiveAmplitude / effectiveWidth + 1);

            result.Add(new Models.StatisticsData("Block", block.Index.ToString()));
            result.Add(new Models.StatisticsData("Trials", block.Targets.Count().ToString()));
            result.Add(new Models.StatisticsData("Amplitude, px", block.Amplitude.ToString()));
            result.Add(new Models.StatisticsData("Width, px", block.Width.ToString()));
            result.Add(new Models.StatisticsData("Offset, px", meanOffset.ToString("F2")));
            result.Add(new Models.StatisticsData("ID, bits", id.ToString("F2")));
            result.Add(new Models.StatisticsData("Eff. Amplitude, px", effectiveAmplitude.ToString("F2")));
            result.Add(new Models.StatisticsData("Eff. Width, px", effectiveWidth.ToString("F2")));
            result.Add(new Models.StatisticsData("Eff. ID, bits", effectiveId.ToString("F2")));
            result.Add(new Models.StatisticsData("MT, ms", meanDuration.ToString()));
            result.Add(new Models.StatisticsData("Errors", errorCount.ToString("F1")));
            result.Add(new Models.StatisticsData("Errors, %", (100.0 * errors).ToString("F1")));
            result.Add(new Models.StatisticsData("Throughput, b/s", throughput.ToString("F2")));
            result.Add(new Models.StatisticsData("", ""));
        }

        return result.ToArray();
    }
}
