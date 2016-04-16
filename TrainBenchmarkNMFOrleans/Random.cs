using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTC2015.TrainBenchmark
{
    /// <summary>
    /// This class exactly reproduces the behavior of Javas Random class
    /// </summary>
    [Serializable]
    public class Random
    {
        public Random(UInt64 seed)
        {
            this.seed = (seed ^ 0x5DEECE66DUL) & ((1UL << 48) - 1);
        }

        public int NextInt(int n)
        {
            if (n <= 0) throw new ArgumentException("n must be positive");

            if ((n & -n) == n)  // i.e., n is a power of 2
                return (int)((n * (long)Next(31)) >> 31);

            long bits, val;
            do
            {
                bits = Next(31);
                val = bits % (UInt32)n;
            }
            while (bits - val + (n - 1) < 0);

            return (int)val;
        }

        protected UInt32 Next(int bits)
        {
            seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);

            return (UInt32)(seed >> (48 - bits));
        }

        private UInt64 seed;
    }
}
