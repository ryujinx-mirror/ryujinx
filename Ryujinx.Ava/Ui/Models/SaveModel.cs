using LibHac;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.Ncm;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.HLE.FileSystem;
using Ryujinx.Ui.App.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ryujinx.Ava.Ui.Models
{
    public class SaveModel : BaseModel
    {
        private readonly HorizonClient _horizonClient;
        private long _size;

        public Action DeleteAction { get; set; }
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

        public string SizeString => $"{((float)_size * 0.000000954):0.###}MB";

        public SaveModel(SaveDataInfo info, HorizonClient horizonClient, VirtualFileSystem virtualFileSystem)
        {
            _horizonClient = horizonClient;
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
                var appMetadata = MainWindow.MainWindowViewModel.ApplicationLibrary.LoadAndSaveMetaData(TitleIdString);
                Title = appMetadata.Title ?? TitleIdString;
            }

            Task.Run(() =>
            {
                var saveRoot = System.IO.Path.Combine(virtualFileSystem.GetNandPath(), $"user/save/{info.SaveDataId:x16}");

                long total_size = GetDirectorySize(saveRoot);
                long GetDirectorySize(string path)
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

                Size = total_size;
            });

        }

        public void OpenLocation()
        {
            ApplicationHelper.OpenSaveDir(SaveId);
        }

        public async void Delete()
        {
            var result = await ContentDialogHelper.CreateConfirmationDialog(LocaleManager.Instance["DeleteUserSave"],
                LocaleManager.Instance["IrreversibleActionNote"],
                LocaleManager.Instance["InputDialogYes"],
                LocaleManager.Instance["InputDialogNo"], "");

            if (result == UserResult.Yes)
            {
                _horizonClient.Fs.DeleteSaveData(SaveDataSpaceId.User, SaveId);

                DeleteAction?.Invoke();
            }
        }
    }
}