using Gtk;
using System;

namespace Ryujinx.Ui.Helper
{
    static class SortHelper
    {
        public static int TimePlayedSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            string aValue = model.GetValue(a, 5).ToString();
            string bValue = model.GetValue(b, 5).ToString();

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

        public static int LastPlayedSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            string aValue = model.GetValue(a, 6).ToString();
            string bValue = model.GetValue(b, 6).ToString();

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

        public static int FileSizeSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            string aValue = model.GetValue(a, 8).ToString();
            string bValue = model.GetValue(b, 8).ToString();

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