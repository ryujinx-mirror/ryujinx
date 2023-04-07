using Gtk;
using System;

namespace Ryujinx.Ui.Helper
{
    static class SortHelper
    {
        public static int TimePlayedSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            static string ReverseFormat(string time)
            {
                if (time == "Never")
                {
                    return "00";
                }

                var numbers = time.Split(new char[] { 'd', 'h', 'm' });

                time = time.Replace(" ", "").Replace("d", ".").Replace("h", ":").Replace("m", "");

                if (numbers.Length == 2)
                {
                    return $"00.00:{time}";
                }
                else if (numbers.Length == 3)
                {
                    return $"00.{time}";
                }

                return time;
            }

            string aValue = ReverseFormat(model.GetValue(a, 5).ToString());
            string bValue = ReverseFormat(model.GetValue(b, 5).ToString());

            return TimeSpan.Compare(TimeSpan.Parse(aValue), TimeSpan.Parse(bValue));
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
