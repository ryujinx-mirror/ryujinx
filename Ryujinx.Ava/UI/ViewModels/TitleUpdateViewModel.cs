using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ns;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.Models;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.Ui.App.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Path = System.IO.Path;
using SpanHelpers = LibHac.Common.SpanHelpers;

namespace Ryujinx.Ava.UI.ViewModels
{
    public class TitleUpdateViewModel : BaseModel
    {
        public TitleUpdateMetadata _titleUpdateWindowData;
        public readonly string     _titleUpdateJsonPath;
        private VirtualFileSystem  _virtualFileSystem { get; }
        private ulong              _titleId           { get; }
        private string             _titleName         { get; }

        private AvaloniaList<TitleUpdateModel> _titleUpdates = new();
        private AvaloniaList<object> _views = new();
        private object _selectedUpdate;

        private static readonly TitleUpdateMetadataJsonSerializerContext SerializerContext = new(JsonHelper.GetDefaultSerializerOptions());

        public AvaloniaList<TitleUpdateModel> TitleUpdates
        {
            get => _titleUpdates;
            set
            {
                _titleUpdates = value;
                OnPropertyChanged();
            }
        }

        public AvaloniaList<object> Views
        {
            get => _views;
            set
            {
                _views = value;
                OnPropertyChanged();
            }
        }

        public object SelectedUpdate
        {
            get => _selectedUpdate;
            set
            {
                _selectedUpdate = value;
                OnPropertyChanged();
            }
        }

        public TitleUpdateViewModel(VirtualFileSystem virtualFileSystem, ulong titleId, string titleName)
        {
            _virtualFileSystem = virtualFileSystem;

            _titleId   = titleId;
            _titleName = titleName;

            _titleUpdateJsonPath = Path.Combine(AppDataManager.GamesDirPath, titleId.ToString("x16"), "updates.json");

            try
            {
                _titleUpdateWindowData = JsonHelper.DeserializeFromFile(_titleUpdateJsonPath, SerializerContext.TitleUpdateMetadata);
            }
            catch
            {
                Logger.Warning?.Print(LogClass.Application, $"Failed to deserialize title update data for {_titleId} at {_titleUpdateJsonPath}");

                _titleUpdateWindowData = new TitleUpdateMetadata
                {
                    Selected = "",
                    Paths    = new List<string>()
                };

                Save();
            }

            LoadUpdates();
        }

        private void LoadUpdates()
        {
            foreach (string path in _titleUpdateWindowData.Paths)
            {
                AddUpdate(path);
            }

            TitleUpdateModel selected = TitleUpdates.FirstOrDefault(x => x.Path == _titleUpdateWindowData.Selected, null);

            SelectedUpdate = selected;

            // NOTE: Save the list again to remove leftovers.
            Save();
            SortUpdates();
        }

        public void SortUpdates()
        {
            var list = TitleUpdates.ToList();

            list.Sort((first, second) =>
            {
                if (string.IsNullOrEmpty(first.Control.DisplayVersionString.ToString()))
                {
                    return -1;
                }
                else if (string.IsNullOrEmpty(second.Control.DisplayVersionString.ToString()))
                {
                    return 1;
                }

                return Version.Parse(first.Control.DisplayVersionString.ToString()).CompareTo(Version.Parse(second.Control.DisplayVersionString.ToString())) * -1;
            });

            Views.Clear();
            Views.Add(new BaseModel());
            Views.AddRange(list);

            if (SelectedUpdate == null)
            {
                SelectedUpdate = Views[0];
            }
            else if (!TitleUpdates.Contains(SelectedUpdate))
            {
                if (Views.Count > 1)
                {
                    SelectedUpdate = Views[1];
                }
                else
                {
                    SelectedUpdate = Views[0];
                }
            }
        }

        private void AddUpdate(string path)
        {
            if (File.Exists(path) && TitleUpdates.All(x => x.Path != path))
            {
                using FileStream file = new(path, FileMode.Open, FileAccess.Read);

                try
                {
                    (Nca patchNca, Nca controlNca) = ApplicationLibrary.GetGameUpdateDataFromPartition(_virtualFileSystem, new PartitionFileSystem(file.AsStorage()), _titleId.ToString("x16"), 0);

                    if (controlNca != null && patchNca != null)
                    {
                        ApplicationControlProperty controlData = new();

                        using UniqueRef<IFile> nacpFile = new();

                        controlNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.None).OpenFile(ref nacpFile.Ref, "/control.nacp".ToU8Span(), OpenMode.Read).ThrowIfFailure();
                        nacpFile.Get.Read(out _, 0, SpanHelpers.AsByteSpan(ref controlData), ReadOption.None).ThrowIfFailure();

                        TitleUpdates.Add(new TitleUpdateModel(controlData, path));
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance[LocaleKeys.DialogUpdateAddUpdateErrorMessage]);
                        });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogLoadNcaErrorMessage, ex.Message, path));
                    });
                }
            }
        }

        public void RemoveUpdate(TitleUpdateModel update)
        {
            TitleUpdates.Remove(update);

            SortUpdates();
        }

        public async void Add()
        {
            OpenFileDialog dialog = new()
            {
                Title         = LocaleManager.Instance[LocaleKeys.SelectUpdateDialogTitle],
                AllowMultiple = true
            };

            dialog.Filters.Add(new FileDialogFilter
            {
                Name       = "NSP",
                Extensions = { "nsp" }
            });

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                string[] files = await dialog.ShowAsync(desktop.MainWindow);

                if (files != null)
                {
                    foreach (string file in files)
                    {
                        AddUpdate(file);
                    }
                }
            }

            SortUpdates();
        }

        public void Save()
        {
            _titleUpdateWindowData.Paths.Clear();
            _titleUpdateWindowData.Selected = "";

            foreach (TitleUpdateModel update in TitleUpdates)
            {
                _titleUpdateWindowData.Paths.Add(update.Path);

                if (update == SelectedUpdate)
                {
                    _titleUpdateWindowData.Selected = update.Path;
                }
            }

            JsonHelper.SerializeToFile(_titleUpdateJsonPath, _titleUpdateWindowData, SerializerContext.TitleUpdateMetadata);
        }
    }
}