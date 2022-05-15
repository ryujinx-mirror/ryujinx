using Ryujinx.Ui.App.Common;
using System.Collections;

namespace Ryujinx.Ava.Ui.Models
{
    public class FileSizeSortComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            string aValue = (x as ApplicationData).TimePlayed;
            string bValue = (y as ApplicationData).TimePlayed;

            if (aValue[^2..] == "GB")
            {
                aValue = (float.Parse(aValue[0..^2]) * 1024).ToString();
            }
            else
            {
                aValue = aValue[0..^2];
            }

            if (bValue[^2..] == "GB")
            {
                bValue = (float.Parse(bValue[0..^2]) * 1024).ToString();
            }
            else
            {
                bValue = bValue[0..^2];
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