using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Profiler.UI
{
    public static class ProfileSorters
    {
        public class InstantAscending : IComparer<KeyValuePair<ProfileConfig, TimingInfo>>
        {
            public int Compare(KeyValuePair<ProfileConfig, TimingInfo> pair1, KeyValuePair<ProfileConfig, TimingInfo> pair2)
                => pair2.Value.Instant.CompareTo(pair1.Value.Instant);
        }

        public class AverageAscending : IComparer<KeyValuePair<ProfileConfig, TimingInfo>>
        {
            public int Compare(KeyValuePair<ProfileConfig, TimingInfo> pair1, KeyValuePair<ProfileConfig, TimingInfo> pair2)
                => pair2.Value.AverageTime.CompareTo(pair1.Value.AverageTime);
        }

        public class TotalAscending : IComparer<KeyValuePair<ProfileConfig, TimingInfo>>
        {
            public int Compare(KeyValuePair<ProfileConfig, TimingInfo> pair1, KeyValuePair<ProfileConfig, TimingInfo> pair2)
                => pair2.Value.TotalTime.CompareTo(pair1.Value.TotalTime);
        }

        public class TagAscending : IComparer<KeyValuePair<ProfileConfig, TimingInfo>>
        {
            public int Compare(KeyValuePair<ProfileConfig, TimingInfo> pair1, KeyValuePair<ProfileConfig, TimingInfo> pair2)
                => StringComparer.CurrentCulture.Compare(pair1.Key.Search, pair2.Key.Search);
        }
    }
}
