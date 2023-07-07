using Ryujinx.Ui.App.Common;
using System;
using System.Collections.Generic;

namespace Ryujinx.Ava.UI.Models.Generic
{
    internal class LastPlayedSortComparer : IComparer<ApplicationData>
    {
        public LastPlayedSortComparer() { }
        public LastPlayedSortComparer(bool isAscending) { IsAscending = isAscending; }

        public bool IsAscending { get; }

        public int Compare(ApplicationData x, ApplicationData y)
        {
            var aValue = x.LastPlayed;
            var bValue = y.LastPlayed;

            if (!aValue.HasValue)
            {
                aValue = DateTime.UnixEpoch;
            }

            if (!bValue.HasValue)
            {
                bValue = DateTime.UnixEpoch;
            }

            return (IsAscending ? 1 : -1) * DateTime.Compare(bValue.Value, aValue.Value);
        }
    }
}
