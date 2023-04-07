using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ARMeilleure
{
    public static class Statistics
    {
        private const int ReportMaxFunctions = 100;

#pragma warning disable CS0169
        [ThreadStatic]
        private static Stopwatch _executionTimer;
#pragma warning restore CS0169

        private static ConcurrentDictionary<ulong, long> _ticksPerFunction;

        static Statistics()
        {
            _ticksPerFunction = new ConcurrentDictionary<ulong, long>();
        }

        public static void InitializeTimer()
        {
#if M_PROFILE
            if (_executionTimer == null)
            {
                _executionTimer = new Stopwatch();
            }
#endif
        }

        internal static void StartTimer()
        {
#if M_PROFILE
            _executionTimer.Restart();
#endif
        }

        internal static void StopTimer(ulong funcAddr)
        {
#if M_PROFILE
            _executionTimer.Stop();

            long ticks = _executionTimer.ElapsedTicks;

            _ticksPerFunction.AddOrUpdate(funcAddr, ticks, (key, oldTicks) => oldTicks + ticks);
#endif
        }

        internal static void ResumeTimer()
        {
#if M_PROFILE
            _executionTimer.Start();
#endif
        }

        internal static void PauseTimer()
        {
#if M_PROFILE
            _executionTimer.Stop();
#endif
        }

        public static string GetReport()
        {
            int count = 0;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(" Function address   | Time");
            sb.AppendLine("--------------------------");

            KeyValuePair<ulong, long>[] funcTable = _ticksPerFunction.ToArray();

            foreach (KeyValuePair<ulong, long> kv in funcTable.OrderByDescending(x => x.Value))
            {
                long timeInMs = (kv.Value * 1000) / Stopwatch.Frequency;

                sb.AppendLine($" 0x{kv.Key:X16} | {timeInMs} ms");

                if (count++ >= ReportMaxFunctions)
                {
                    break;
                }
            }

            return sb.ToString();
        }
    }
}