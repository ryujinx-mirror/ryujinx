using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ui.App.Common;
using System;
using System.Collections.Generic;

namespace Ryujinx.Ava.Ui.Models.Generic
{
    internal class LastPlayedSortComparer : IComparer<ApplicationData>
    {
        public LastPlayedSortComparer() { }
        public LastPlayedSortComparer(bool isAscending) { IsAscending = isAscending; }

        public bool IsAscending { get; }

        public int Compare(ApplicationData x, ApplicationData y)
        {
            string aValue = x.LastPlayed;
            string bValue = y.LastPlayed;

            if (aValue == LocaleManager.Instance["Never"])
            {
                aValue = DateTime.UnixEpoch.ToString();
            }

            if (bValue == LocaleManager.Instance["Never"])
            {
                bValue = DateTime.UnixEpoch.ToString();
            }

            return (IsAscending ? 1 : -1) * DateTime.Compare(DateTime.Parse(bValue), DateTime.Parse(aValue));
        }
    }
}