using Ryujinx.Ui.App.Common;
using System.Collections.Generic;

namespace Ryujinx.Ava.Ui.Models.Generic
{
    internal class FileSizeSortComparer : IComparer<ApplicationData>
    {
        public FileSizeSortComparer() { }
        public FileSizeSortComparer(bool isAscending) { _order = isAscending ? 1 : -1; }

        private int _order;

        public int Compare(ApplicationData x, ApplicationData y)
        {
            string aValue = x.FileSize;
            string bValue = y.FileSize;

            if (aValue[^3..] == "GiB")
            {
                aValue = (float.Parse(aValue[0..^3]) * 1024).ToString();
            }
            else
            {
                aValue = aValue[0..^3];
            }

            if (bValue[^3..] == "GiB")
            {
                bValue = (float.Parse(bValue[0..^3]) * 1024).ToString();
            }
            else
            {
                bValue = bValue[0..^3];
            }

            if (float.Parse(aValue) > float.Parse(bValue))
            {
                return -1 * _order;
            }
            else if (float.Parse(bValue) > float.Parse(aValue))
            {
                return 1 * _order;
            }
            else
            {
                return 0;
            }
        }
    }
}