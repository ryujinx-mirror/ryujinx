using LibHac.Fs;
using LibHac.Ncm;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.Ava.UI.Windows;
using Ryujinx.Ui.App.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Ryujinx.Ava.UI.Models
{
    public class SaveModel : BaseModel
    {
        private long _size;

        public ulong SaveId { get; }
        public ProgramId TitleId { get; }
        public string TitleIdString => $"{TitleId.Value:X16}";
        public UserId UserId { get; }
        public bool InGameList { get; }
        public string Title { get; }
        public byte[] Icon { get; }

        public long Size
        {
            get => _size; set
            {
                _size = value;
                SizeAvailable = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SizeString));
                OnPropertyChanged(nameof(SizeAvailable));
            }
        }

        public bool SizeAvailable { get; set; }

        public string SizeString => GetSizeString();

        private string GetSizeString()
        {
            const int Scale = 1024;
            string[] orders = { "GiB", "MiB", "KiB" };
            long max = (long)Math.Pow(Scale, orders.Length);

            foreach (string order in orders)
            {
                if (Size > max)
                {
                    return $"{decimal.Divide(Size, max):##.##} {order}";
                }

                max /= Scale;
            }

            return "0 KiB";
        }

        public SaveModel(SaveDataInfo info)
        {
            SaveId = info.SaveDataId;
            TitleId = info.ProgramId;
            UserId = info.UserId;

            var appData = MainWindow.MainWindowViewModel.Applications.FirstOrDefault(x => x.TitleId.ToUpper() == TitleIdString);

            InGameList = appData != null;

            if (InGameList)
            {
                Icon = appData.Icon;
                Title = appData.TitleName;
            }
            else
            {
                var appMetadata = ApplicationLibrary.LoadAndSaveMetaData(TitleIdString);
                Title = appMetadata.Title ?? TitleIdString;
            }

            Task.Run(() =>
            {
                var saveRoot = Path.Combine(MainWindow.MainWindowViewModel.VirtualFileSystem.GetNandPath(), $"user/save/{info.SaveDataId:x16}");

                long totalSize = GetDirectorySize(saveRoot);

                static long GetDirectorySize(string path)
                {
                    long size = 0;
                    if (Directory.Exists(path))
                    {
                        var directories = Directory.GetDirectories(path);
                        foreach (var directory in directories)
                        {
                            size += GetDirectorySize(directory);
                        }

                        var files = Directory.GetFiles(path);
                        foreach (var file in files)
                        {
                            size += new FileInfo(file).Length;
                        }
                    }

                    return size;
                }

                Size = totalSize;
            });

        }
    }
}
