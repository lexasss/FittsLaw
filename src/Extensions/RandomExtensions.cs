namespace FittsLaw.Extensions;

internal static class RandomExtensions
{
#if VS_VERSION_18_0_OR_GREATER
    extension(Random rnd)
    {
        public Random Shuffle<T>(IList<T> array)
        {
            void Shuffle()
            {
                int n = array.Count;
                while (n > 1)
                {
                    int k = rnd.Next(n--);
                    (array[k], array[n]) = (array[n], array[k]);
                }
            }

            int repetitions = rnd.Next(8) + 3;  // 3..10 repetitions
            for (int i = 0; i < repetitions; i++)
            {
                Shuffle();
            }

            return rnd;
        }
    }
#else
    public static Random Shuffle<T>(this Random rnd, IList<T> array)
    {
        void Shuffle()
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = rnd.Next(n--);
                (array[k], array[n]) = (array[n], array[k]);
            }
        }

        int repetitions = rnd.Next(8) + 3;  // 3..10 repetitions
        for (int i = 0; i < repetitions; i++)
        {
            Shuffle();
        }

        return rnd;
    }
#endif
}