using System;

namespace ARMeilleure.Translation.PTC
{
    class DegreeOfParallelism
    {
        public double GiBRef { get; } // GiB.
        public double WeightRef { get; } // %.
        public double IncrementByGiB { get; } // %.
        private double _coefficient;

        public DegreeOfParallelism(double gibRef, double weightRef, double incrementByGiB)
        {
            GiBRef = gibRef;
            WeightRef = weightRef;
            IncrementByGiB = incrementByGiB;

            _coefficient = weightRef - (incrementByGiB * gibRef);
        }

        public int GetDegreeOfParallelism(int min, int max)
        {
            double degreeOfParallelism = (GetProcessorCount() * GetWeight(GetAvailableMemoryGiB())) / 100d;

            return Math.Clamp((int)Math.Round(degreeOfParallelism), min, max);
        }

        public static double GetProcessorCount()
        {
            return (double)Environment.ProcessorCount;
        }

        public double GetWeight(double gib)
        {
            return (IncrementByGiB * gib) + _coefficient;
        }

        public static double GetAvailableMemoryGiB()
        {
            GCMemoryInfo gcMemoryInfo = GC.GetGCMemoryInfo();

            return FromBytesToGiB(gcMemoryInfo.TotalAvailableMemoryBytes - gcMemoryInfo.MemoryLoadBytes);
        }

        private static double FromBytesToGiB(long bytes)
        {
            return Math.ScaleB((double)bytes, -30);
        }
    }
}