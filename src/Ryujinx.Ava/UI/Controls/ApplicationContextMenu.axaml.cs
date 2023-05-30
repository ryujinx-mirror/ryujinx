using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LibHac.Fs;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.Ava.UI.Windows;
using Ryujinx.Common.Configuration;
using Ryujinx.Ui.App.Common;
using Ryujinx.HLE.HOS;
using Ryujinx.Ui.Common.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Path = System.IO.Path;
using UserId = LibHac.Fs.UserId;

namespace Ryujinx.Ava.UI.Controls
{
    public class ApplicationContextMenu : MenuFlyout
    {
        public ApplicationContextMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ToggleFavorite_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                viewModel.SelectedApplication.Favorite = !viewModel.SelectedApplication.Favorite;

                viewModel.ApplicationLibrary.LoadAndSaveMetaData(viewModel.SelectedApplication.TitleId, appMetadata =>
                {
                    appMetadata.Favorite = viewModel.SelectedApplication.Favorite;
                });

                viewModel.RefreshView();
            }
        }

        public void OpenUserSaveDirectory_Click(object sender, RoutedEventArgs args)
        {
            if ((sender as MenuItem)?.DataContext is MainWindowViewModel viewModel)
            {
                OpenSaveDirectory(viewModel, SaveDataType.Account, userId: new UserId((ulong)viewModel.AccountManager.LastOpenedUser.UserId.High, (ulong)viewModel.AccountManager.LastOpenedUser.UserId.Low));
            }
        }

        public void OpenDeviceSaveDirectory_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            OpenSaveDirectory(viewModel, SaveDataType.Device, userId: default);
        }

        public void OpenBcatSaveDirectory_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            OpenSaveDirectory(viewModel, SaveDataType.Bcat, userId: default);
        }

        private static void OpenSaveDirectory(MainWindowViewModel viewModel, SaveDataType saveDataType, UserId userId)
        {
            if (viewModel?.SelectedApplication != null)
            {
                if (!ulong.TryParse(viewModel.SelectedApplication.TitleId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleIdNumber))
                {
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance[LocaleKeys.DialogRyujinxErrorMessage], LocaleManager.Instance[LocaleKeys.DialogInvalidTitleIdErrorMessage]);
                    });

                    return;
                }

                var saveDataFilter = SaveDataFilter.Make(titleIdNumber, saveDataType, userId, saveDataId: default, index: default);

                ApplicationHelper.OpenSaveDir(in saveDataFilter, titleIdNumber, viewModel.SelectedApplication.ControlHolder, viewModel.SelectedApplication.TitleName);
            }
        }

        public async void OpenTitleUpdateManager_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                await TitleUpdateWindow.Show(viewModel.VirtualFileSystem, ulong.Parse(viewModel.SelectedApplication.TitleId, NumberStyles.HexNumber), viewModel.SelectedApplication.TitleName);
            }
        }

        public async void OpenDownloadableContentManager_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                await DownloadableContentManagerWindow.Show(viewModel.VirtualFileSystem, ulong.Parse(viewModel.SelectedApplication.TitleId, NumberStyles.HexNumber), viewModel.SelectedApplication.TitleName);
            }
        }

        public async void OpenCheatManager_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                await new CheatWindow(
                    viewModel.VirtualFileSystem,
                    viewModel.SelectedApplication.TitleId,
                    viewModel.SelectedApplication.TitleName,
                    viewModel.SelectedApplication.Path).ShowDialog(viewModel.TopLevel as Window);
            }
        }

        public void OpenModsDirectory_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                string modsBasePath = ModLoader.GetModsBasePath();
                string titleModsPath = ModLoader.GetTitleDir(modsBasePath, viewModel.SelectedApplication.TitleId);

                OpenHelper.OpenFolder(titleModsPath);
            }
        }

        public void OpenSdModsDirectory_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                string sdModsBasePath = ModLoader.GetSdModsBasePath();
                string titleModsPath = ModLoader.GetTitleDir(sdModsBasePath, viewModel.SelectedApplication.TitleId);

                OpenHelper.OpenFolder(titleModsPath);
            }
        }

        public async void PurgePtcCache_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(LocaleManager.Instance[LocaleKeys.DialogWarning],
                                                                                       LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogPPTCDeletionMessage, viewModel.SelectedApplication.TitleName),
                                                                                       LocaleManager.Instance[LocaleKeys.InputDialogYes],
                                                                                       LocaleManager.Instance[LocaleKeys.InputDialogNo],
                                                                                       LocaleManager.Instance[LocaleKeys.RyujinxConfirm]);

                if (result == UserResult.Yes)
                {
                    DirectoryInfo mainDir = new(Path.Combine(AppDataManager.GamesDirPath, viewModel.SelectedApplication.TitleId, "cache", "cpu", "0"));
                    DirectoryInfo backupDir = new(Path.Combine(AppDataManager.GamesDirPath, viewModel.SelectedApplication.TitleId, "cache", "cpu", "1"));

                    List<FileInfo> cacheFiles = new();

                    if (mainDir.Exists)
                    {
                        cacheFiles.AddRange(mainDir.EnumerateFiles("*.cache"));
                    }

                    if (backupDir.Exists)
                    {
                        cacheFiles.AddRange(backupDir.EnumerateFiles("*.cache"));
                    }

                    if (cacheFiles.Count > 0)
                    {
                        foreach (FileInfo file in cacheFiles)
                        {
                            try
                            {
                                file.Delete();
                            }
                            catch (Exception ex)
                            {
                                await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogPPTCDeletionErrorMessage, file.Name, ex));
                            }
                        }
                    }
                }
            }
        }

        public async void PurgeShaderCache_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(LocaleManager.Instance[LocaleKeys.DialogWarning],
                                                                                       LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogShaderDeletionMessage, viewModel.SelectedApplication.TitleName),
                                                                                       LocaleManager.Instance[LocaleKeys.InputDialogYes],
                                                                                       LocaleManager.Instance[LocaleKeys.InputDialogNo],
                                                                                       LocaleManager.Instance[LocaleKeys.RyujinxConfirm]);

                if (result == UserResult.Yes)
                {
                    DirectoryInfo shaderCacheDir = new(Path.Combine(AppDataManager.GamesDirPath, viewModel.SelectedApplication.TitleId, "cache", "shader"));

                    List<DirectoryInfo> oldCacheDirectories = new();
                    List<FileInfo> newCacheFiles = new();

                    if (shaderCacheDir.Exists)
                    {
                        oldCacheDirectories.AddRange(shaderCacheDir.EnumerateDirectories("*"));
                        newCacheFiles.AddRange(shaderCacheDir.GetFiles("*.toc"));
                        newCacheFiles.AddRange(shaderCacheDir.GetFiles("*.data"));
                    }

                    if ((oldCacheDirectories.Count > 0 || newCacheFiles.Count > 0))
                    {
                        foreach (DirectoryInfo directory in oldCacheDirectories)
                        {
                            try
                            {
                                directory.Delete(true);
                            }
                            catch (Exception ex)
                            {
                                await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.DialogPPTCDeletionErrorMessage, directory.Name, ex));
                            }
                        }

                        foreach (FileInfo file in newCacheFiles)
                        {
                            try
                            {
                                file.Delete();
                            }
                            catch (Exception ex)
                            {
                                await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.ShaderCachePurgeError, file.Name, ex));
                            }
                        }
                    }
                }
            }
        }

        public void OpenPtcDirectory_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                string ptcDir = Path.Combine(AppDataManager.GamesDirPath, viewModel.SelectedApplication.TitleId, "cache", "cpu");
                string mainDir = Path.Combine(ptcDir, "0");
                string backupDir = Path.Combine(ptcDir, "1");

                if (!Directory.Exists(ptcDir))
                {
                    Directory.CreateDirectory(ptcDir);
                    Directory.CreateDirectory(mainDir);
                    Directory.CreateDirectory(backupDir);
                }

                OpenHelper.OpenFolder(ptcDir);
            }
        }

        public void OpenShaderCacheDirectory_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                string shaderCacheDir = Path.Combine(AppDataManager.GamesDirPath, viewModel.SelectedApplication.TitleId, "cache", "shader");

                if (!Directory.Exists(shaderCacheDir))
                {
                    Directory.CreateDirectory(shaderCacheDir);
                }

                OpenHelper.OpenFolder(shaderCacheDir);
            }
        }

        public async void ExtractApplicationExeFs_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                await ApplicationHelper.ExtractSection(NcaSectionType.Code, viewModel.SelectedApplication.Path, viewModel.SelectedApplication.TitleName);
            }
        }

        public async void ExtractApplicationRomFs_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                await ApplicationHelper.ExtractSection(NcaSectionType.Data, viewModel.SelectedApplication.Path, viewModel.SelectedApplication.TitleName);
            }
        }

        public async void ExtractApplicationLogo_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                await ApplicationHelper.ExtractSection(NcaSectionType.Logo, viewModel.SelectedApplication.Path, viewModel.SelectedApplication.TitleName);
            }
        }

        public void RunApplication_Click(object sender, RoutedEventArgs args)
        {
            var viewModel = (sender as MenuItem)?.DataContext as MainWindowViewModel;

            if (viewModel?.SelectedApplication != null)
            {
                viewModel.LoadApplication(viewModel.SelectedApplication.Path);
            }
        }
    }
}