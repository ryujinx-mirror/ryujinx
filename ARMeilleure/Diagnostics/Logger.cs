using ARMeilleure.Translation;
using System;
using System.Diagnostics;

namespace ARMeilleure.Diagnostics
{
    static class Logger
    {
        private static long _startTime;

        private static long[] _accumulatedTime;

        static Logger()
        {
            _accumulatedTime = new long[(int)PassName.Count];
        }

        public static void StartPass(PassName name)
        {
#if M_DEBUG
            WriteOutput(name + " pass started...");

            _startTime = Stopwatch.GetTimestamp();
#endif
        }

        public static void EndPass(PassName name, ControlFlowGraph cfg)
        {
#if M_DEBUG
            EndPass(name);

            WriteOutput("IR after " + name + " pass:");

            WriteOutput(IRDumper.GetDump(cfg));
#endif
        }

        public static void EndPass(PassName name)
        {
#if M_DEBUG
            long elapsedTime = Stopwatch.GetTimestamp() - _startTime;

            _accumulatedTime[(int)name] += elapsedTime;

            WriteOutput($"{name} pass ended after {GetMilliseconds(_accumulatedTime[(int)name])} ms...");
#endif
        }

        private static long GetMilliseconds(long ticks)
        {
            return (long)(((double)ticks / Stopwatch.Frequency) * 1000);
        }

        private static void WriteOutput(string text)
        {
            Console.WriteLine(text);
        }
    }
}