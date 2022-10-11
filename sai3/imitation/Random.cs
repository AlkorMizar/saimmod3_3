using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sai3.random
{
    internal class Random
    {
        long R0;
        double previousR;
        double m, a;
        public long M { get => (long)m; }

        public Random()
        {
            R0 = 15;
            m = 12031278;
            a = 1643;
        }
        public Random(long R0, long m, long a)
        {
            if (m < a)
                throw new ArgumentException($"a>m : {a}>{m}");
            previousR = this.R0 = R0;
            this.a = a;
            this.m = m;
        }

        private Random(long R0, double prev, long m, long a) : this(R0, m, a)
        {
            previousR = prev;
        }

        public double NextDouble()
        {
            previousR = (previousR * a) % m;
            var res = (float)(previousR / m);
            return res;
        }
    }
}
