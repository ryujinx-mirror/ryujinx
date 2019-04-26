using Ryujinx.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryujinx.Profiler
{
    public static class Profile
    {
        public static float UpdateRate    => _settings.UpdateRate;
        public static long  HistoryLength => _settings.History;

        public static ProfilerKeyboardHandler Controls      => _settings.Controls;

        private static InternalProfile  _profileInstance;
        private static ProfilerSettings _settings;

        [Conditional("USE_PROFILING")]
        public static void Initalize()
        {
            var config = ProfilerConfiguration.Load("ProfilerConfig.jsonc");

            _settings = new ProfilerSettings()
            {
                Enabled         = config.Enabled,
                FileDumpEnabled = config.DumpPath != "",
                DumpLocation    = config.DumpPath,
                UpdateRate      = (config.UpdateRate <= 0) ? -1 : 1.0f / config.UpdateRate,
                History         = (long)(config.History * PerformanceCounter.TicksPerSecond),
                MaxLevel        = config.MaxLevel,
                Controls        = config.Controls,
                MaxFlags        = config.MaxFlags,
            };
        }

        public static bool ProfilingEnabled()
        {
#if USE_PROFILING
            if (!_settings.Enabled)
                return false;

            if (_profileInstance == null)
                _profileInstance = new InternalProfile(_settings.History, _settings.MaxFlags);

            return true;
#else
            return false;
#endif
        }

        [Conditional("USE_PROFILING")]
        public static void FinishProfiling()
        {
            if (!ProfilingEnabled())
                return;

            if (_settings.FileDumpEnabled)
                DumpProfile.ToFile(_settings.DumpLocation, _profileInstance);

            _profileInstance.Dispose();
        }

        [Conditional("USE_PROFILING")]
        public static void FlagTime(TimingFlagType flagType)
        {
            if (!ProfilingEnabled())
                return;
            _profileInstance.FlagTime(flagType);
        }

        [Conditional("USE_PROFILING")]
        public static void RegisterFlagReciever(Action<TimingFlag> reciever)
        {
            if (!ProfilingEnabled())
                return;
            _profileInstance.RegisterFlagReciever(reciever);
        }

        [Conditional("USE_PROFILING")]
        public static void Begin(ProfileConfig config)
        {
            if (!ProfilingEnabled())
                return;
            if (config.Level > _settings.MaxLevel)
                return;
            _profileInstance.BeginProfile(config);
        }

        [Conditional("USE_PROFILING")]
        public static void End(ProfileConfig config)
        {
            if (!ProfilingEnabled())
                return;
            if (config.Level > _settings.MaxLevel)
                return;
            _profileInstance.EndProfile(config);
        }

        public static string GetSession()
        {
#if USE_PROFILING
            if (!ProfilingEnabled())
                return null;
            return _profileInstance.GetSession();
#else
            return "";
#endif
        }

        public static List<KeyValuePair<ProfileConfig, TimingInfo>> GetProfilingData()
        {
#if USE_PROFILING
            if (!ProfilingEnabled())
                return new List<KeyValuePair<ProfileConfig, TimingInfo>>();
            return _profileInstance.GetProfilingData();
#else
            return new List<KeyValuePair<ProfileConfig, TimingInfo>>();
#endif
        }

        public static TimingFlag[] GetTimingFlags()
        {
#if USE_PROFILING
            if (!ProfilingEnabled())
                return new TimingFlag[0];
            return _profileInstance.GetTimingFlags();
#else
            return new TimingFlag[0];
#endif
        }

        public static (long[], long[]) GetTimingAveragesAndLast()
        {
#if USE_PROFILING
            if (!ProfilingEnabled())
                return (new long[0], new long[0]);
            return _profileInstance.GetTimingAveragesAndLast();
#else
            return (new long[0], new long[0]);
#endif
        }
    }
}
