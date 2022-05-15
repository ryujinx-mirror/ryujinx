using Ryujinx.Ui.App.Common;
using System.Collections;

namespace Ryujinx.Ava.Ui.Models
{
    public class TimePlayedSortComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            string aValue = (x as ApplicationData).TimePlayed;
            string bValue = (y as ApplicationData).TimePlayed;

            if (aValue.Length > 4 && aValue[^4..] == "mins")
            {
                aValue = (float.Parse(aValue[0..^5]) * 60).ToString();
            }
            else if (aValue.Length > 3 && aValue[^3..] == "hrs")
            {
                aValue = (float.Parse(aValue[0..^4]) * 3600).ToString();
            }
            else if (aValue.Length > 4 && aValue[^4..] == "days")
            {
                aValue = (float.Parse(aValue[0..^5]) * 86400).ToString();
            }
            else
            {
                aValue = aValue[0..^1];
            }

            if (bValue.Length > 4 && bValue[^4..] == "mins")
            {
                bValue = (float.Parse(bValue[0..^5]) * 60).ToString();
            }
            else if (bValue.Length > 3 && bValue[^3..] == "hrs")
            {
                bValue = (float.Parse(bValue[0..^4]) * 3600).ToString();
            }
            else if (bValue.Length > 4 && bValue[^4..] == "days")
            {
                bValue = (float.Parse(bValue[0..^5]) * 86400).ToString();
            }
            else
            {
                bValue = bValue[0..^1];
            }

            if (float.Parse(aValue) > float.Parse(bValue))
            {
                return -1;
            }
            else if (float.Parse(bValue) > float.Parse(aValue))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}