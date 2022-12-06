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
            float aFloat;
            float bFloat;

            if (aValue.Length > 7 && aValue[^7..] == "minutes")
            {
                aValue = aValue.Replace("minutes", "");
                aFloat = (float.Parse(aValue) * 60);
            }
            else if (aValue.Length > 5 && aValue[^5..] == "hours")
            {
                aValue = aValue.Replace("hours", "");
                aFloat = (float.Parse(aValue) * 3600);
            }
            else if (aValue.Length > 4 && aValue[^4..] == "days")
            {
                aValue = aValue.Replace("days", "");
                aFloat = (float.Parse(aValue) * 86400);
            }
            else
            {
                aValue = aValue.Replace("seconds", "");
                aFloat = float.Parse(aValue);
            }

            if (bValue.Length > 7 && bValue[^7..] == "minutes")
            {
                bValue = bValue.Replace("minutes", "");
                bFloat = (float.Parse(bValue) * 60);
            }
            else if (bValue.Length > 5 && bValue[^5..] == "hours")
            {
                bValue = bValue.Replace("hours", "");
                bFloat = (float.Parse(bValue) * 3600);
            }
            else if (bValue.Length > 4 && bValue[^4..] == "days")
            {
                bValue =  bValue.Replace("days", "");
                bFloat = (float.Parse(bValue) * 86400);
            }
            else
            {
                bValue = bValue[0..^8];
                bFloat = float.Parse(bValue);
            }

            if (aFloat > bFloat)
            {
                return -1;
            }
            else if (bFloat > aFloat)
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