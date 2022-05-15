using Ryujinx.Ui.App.Common;
using System;
using System.Collections;

namespace Ryujinx.Ava.Ui.Models
{
    public class LastPlayedSortComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            string aValue = (x as ApplicationData).LastPlayed;
            string bValue = (y as ApplicationData).LastPlayed;

            if (aValue == "Never")
            {
                aValue = DateTime.UnixEpoch.ToString();
            }

            if (bValue == "Never")
            {
                bValue = DateTime.UnixEpoch.ToString();
            }

            return DateTime.Compare(DateTime.Parse(bValue), DateTime.Parse(aValue));
        }
    }
}