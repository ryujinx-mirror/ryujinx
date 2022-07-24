using Avalonia.Threading;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Ryujinx.Ava;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.Modules
{
    internal static class Updater
    {
        private const string GitHubApiURL = "https://api.github.com";
        internal static bool Running;

        private static readonly string HomeDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string UpdateDir = Path.Combine(Path.GetTempPath(), "Ryujinx", "update");
        private static readonly string UpdatePublishDir = Path.Combine(UpdateDir, "publish");
        private static readonly int ConnectionCount = 4;

        private static string _buildVer;
        private static string _platformExt;
        private static string _buildUrl;
        private static long _buildSize;

        private static readonly string[] WindowsDependencyDirs = Array.Empty<string>();

        public static async Task BeginParse(MainWindow mainWindow, bool showVersionUpToDate)
        {
            if (Running)
            {
                return;
            }

            Running = true;
            mainWindow.CanUpdate = false;

            // Detect current platform
            if (OperatingSystem.IsMacOS())
            {
                _platformExt = "osx_x64.zip";
            }
            else if (OperatingSystem.IsWindows())
            {
                _platformExt = "win_x64.zip";
            }
            else if (OperatingSystem.IsLinux())
            {
                _platformExt = "linux_x64.tar.gz";
            }

            Version newVersion;
            Version currentVersion;

            try
            {
                currentVersion = Version.Parse(Program.Version);
            }
            catch
            {
                Logger.Error?.Print(LogClass.Application, "Failed to convert the current Ryujinx version!");
                Dispatcher.UIThread.Post(async () =>
                {
                    await ContentDialogHelper.CreateWarningDialog(LocaleManager.Instance["DialogUpdaterConvertFailedMessage"], LocaleManager.Instance["DialogUpdaterCancelUpdateMessage"]);
                });

                return;
            }

            // Get latest version number from GitHub API
            try
            {
                using (HttpClient jsonClient = ConstructHttpClient())
                {
                    string buildInfoURL = $"{GitHubApiURL}/repos/{ReleaseInformations.ReleaseChannelOwner}/{ReleaseInformations.ReleaseChannelRepo}/releases/latest";

                    string fetchedJson = await jsonClient.GetStringAsync(buildInfoURL);
                    JObject jsonRoot = JObject.Parse(fetchedJson);
                    JToken assets = jsonRoot["assets"];

                    _buildVer = (string)jsonRoot["name"];

                    foreach (JToken asset in assets)
                    {
                        string assetName = (string)asset["name"];
                        string assetState = (string)asset["state"];
                        string downloadURL = (string)asset["browser_download_url"];

                        if (assetName.StartsWith("test-ava-ryujinx") && assetName.EndsWith(_platformExt))
                        {
                            _buildUrl = downloadURL;

                            if (assetState != "uploaded")
                            {
                                if (showVersionUpToDate)
                                {
                                    Dispatcher.UIThread.Post(async () =>
                                    {
                                        await ContentDialogHelper.CreateUpdaterInfoDialog(LocaleManager.Instance["DialogUpdaterAlreadyOnLatestVersionMessage"], "");
                                    });
                                }

                                return;
                            }

                            break;
                        }
                    }

                    // If build not done, assume no new update are availaible.
                    if (_buildUrl == null)
                    {
                        if (showVersionUpToDate)
                        {
                            Dispatcher.UIThread.Post(async () =>
                            {
                                await ContentDialogHelper.CreateUpdaterInfoDialog(LocaleManager.Instance["DialogUpdaterAlreadyOnLatestVersionMessage"], "");
                            });
                        }

                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error?.Print(LogClass.Application, exception.Message);
                Dispatcher.UIThread.Post(async () =>
                {
                    await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance["DialogUpdaterFailedToGetVersionMessage"]);
                });

                return;
            }

            try
            {
                newVersion = Version.Parse(_buildVer);
            }
            catch
            {
                Logger.Error?.Print(LogClass.Application, "Failed to convert the received Ryujinx version from Github!");
                Dispatcher.UIThread.Post(async () =>
                {
                    await ContentDialogHelper.CreateWarningDialog(LocaleManager.Instance["DialogUpdaterConvertFailedGithubMessage"], LocaleManager.Instance["DialogUpdaterCancelUpdateMessage"]);
                });

                return;
            }

            if (newVersion <= currentVersion)
            {
                if (showVersionUpToDate)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        await ContentDialogHelper.CreateUpdaterInfoDialog(LocaleManager.Instance["DialogUpdaterAlreadyOnLatestVersionMessage"], "");
                    });
                }

                Running = false;
                mainWindow.CanUpdate = true;

                return;
            }

            // Fetch build size information to learn chunk sizes.
            using (HttpClient buildSizeClient = ConstructHttpClient())
            {
                try
                {
                    buildSizeClient.DefaultRequestHeaders.Add("Range", "bytes=0-0");

                    HttpResponseMessage message = await buildSizeClient.GetAsync(new Uri(_buildUrl), HttpCompletionOption.ResponseHeadersRead);

                    _buildSize = message.Content.Headers.ContentRange.Length.Value;
                }
                catch (Exception ex)
                {
                    Logger.Warning?.Print(LogClass.Application, ex.Message);
                    Logger.Warning?.Print(LogClass.Application, "Couldn't determine build size for update, using single-threaded updater");

                    _buildSize = -1;
                }
            }
            Dispatcher.UIThread.Post(async () =>
            {
                // Show a message asking the user if they want to update
                UpdaterWindow updateDialog = new(mainWindow, newVersion, _buildUrl);
                await updateDialog.ShowDialog(mainWindow);
            });
        }

        private static HttpClient ConstructHttpClient()
        {
            HttpClient result = new HttpClient();

            // Required by GitHub to interract with APIs.
            result.DefaultRequestHeaders.Add("User-Agent", "Ryujinx-Updater/1.0.0");

            return result;
        }

        public static void UpdateRyujinx(UpdaterWindow updateDialog, string downloadUrl)
        {
            // Empty update dir, although it shouldn't ever have anything inside it
            if (Directory.Exists(UpdateDir))
            {
                Directory.Delete(UpdateDir, true);
            }

            Directory.CreateDirectory(UpdateDir);

            string updateFile = Path.Combine(UpdateDir, "update.bin");

            // Download the update .zip
            updateDialog.MainText.Text = LocaleManager.Instance["UpdaterDownloading"];
            updateDialog.ProgressBar.Value = 0;
            updateDialog.ProgressBar.Maximum = 100;

            Task.Run(() =>
            {
                if (_buildSize >= 0)
                {
                    DoUpdateWithMultipleThreads(updateDialog, downloadUrl, updateFile);
                }
                else
                {
                    DoUpdateWithSingleThread(updateDialog, downloadUrl, updateFile);
                }
            });
        }

        private static void DoUpdateWithMultipleThreads(UpdaterWindow updateDialog, string downloadUrl, string updateFile)
        {
            // Multi-Threaded Updater
            long chunkSize = _buildSize / ConnectionCount;
            long remainderChunk = _buildSize % ConnectionCount;

            int completedRequests = 0;
            int totalProgressPercentage = 0;
            int[] progressPercentage = new int[ConnectionCount];

            List<byte[]> list = new List<byte[]>(ConnectionCount);
            List<WebClient> webClients = new List<WebClient>(ConnectionCount);

            for (int i = 0; i < ConnectionCount; i++)
            {
                list.Add(Array.Empty<byte>());
            }

            for (int i = 0; i < ConnectionCount; i++)
            {
#pragma warning disable SYSLIB0014
                // TODO: WebClient is obsolete and need to be replaced with a more complex logic using HttpClient.
                using (WebClient client = new WebClient())
#pragma warning restore SYSLIB0014
                {
                    webClients.Add(client);

                    if (i == ConnectionCount - 1)
                    {
                        client.Headers.Add("Range", $"bytes={chunkSize * i}-{(chunkSize * (i + 1) - 1) + remainderChunk}");
                    }
                    else
                    {
                        client.Headers.Add("Range", $"bytes={chunkSize * i}-{chunkSize * (i + 1) - 1}");
                    }

                    client.DownloadProgressChanged += (_, args) =>
                    {
                        int index = (int)args.UserState;

                        Interlocked.Add(ref totalProgressPercentage, -1 * progressPercentage[index]);
                        Interlocked.Exchange(ref progressPercentage[index], args.ProgressPercentage);
                        Interlocked.Add(ref totalProgressPercentage, args.ProgressPercentage);

                        updateDialog.ProgressBar.Value = totalProgressPercentage / ConnectionCount;
                    };

                    client.DownloadDataCompleted += (_, args) =>
                    {
                        int index = (int)args.UserState;

                        if (args.Cancelled)
                        {
                            webClients[index].Dispose();

                            return;
                        }

                        list[index] = args.Result;
                        Interlocked.Increment(ref completedRequests);

                        if (Interlocked.Equals(completedRequests, ConnectionCount))
                        {
                            byte[] mergedFileBytes = new byte[_buildSize];
                            for (int connectionIndex = 0, destinationOffset = 0; connectionIndex < ConnectionCount; connectionIndex++)
                            {
                                Array.Copy(list[connectionIndex], 0, mergedFileBytes, destinationOffset, list[connectionIndex].Length);
                                destinationOffset += list[connectionIndex].Length;
                            }

                            File.WriteAllBytes(updateFile, mergedFileBytes);

                            try
                            {
                                InstallUpdate(updateDialog, updateFile);
                            }
                            catch (Exception e)
                            {
                                Logger.Warning?.Print(LogClass.Application, e.Message);
                                Logger.Warning?.Print(LogClass.Application, "Multi-Threaded update failed, falling back to single-threaded updater.");

                                DoUpdateWithSingleThread(updateDialog, downloadUrl, updateFile);

                                return;
                            }
                        }
                    };

                    try
                    {
                        client.DownloadDataAsync(new Uri(downloadUrl), i);
                    }
                    catch (WebException ex)
                    {
                        Logger.Warning?.Print(LogClass.Application, ex.Message);
                        Logger.Warning?.Print(LogClass.Application, "Multi-Threaded update failed, falling back to single-threaded updater.");

                        for (int j = 0; j < webClients.Count; j++)
                        {
                            webClients[j].CancelAsync();
                        }

                        DoUpdateWithSingleThread(updateDialog, downloadUrl, updateFile);

                        return;
                    }
                }
            }
        }

        private static void DoUpdateWithSingleThreadWorker(UpdaterWindow updateDialog, string downloadUrl, string updateFile)
        {
            using (HttpClient client = new HttpClient())
            {
                // We do not want to timeout while downloading
                client.Timeout = TimeSpan.FromDays(1);

                using (HttpResponseMessage response = client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result)
                using (Stream remoteFileStream = response.Content.ReadAsStreamAsync().Result)
                {
                    using (Stream updateFileStream = File.Open(updateFile, FileMode.Create))
                    {
                        long totalBytes = response.Content.Headers.ContentLength.Value;
                        long byteWritten = 0;

                        byte[] buffer = new byte[32 * 1024];

                        while (true)
                        {
                            int readSize = remoteFileStream.Read(buffer);

                            if (readSize == 0)
                            {
                                break;
                            }

                            byteWritten += readSize;

                            updateDialog.ProgressBar.Value = ((double)byteWritten / totalBytes) * 100;
                            updateFileStream.Write(buffer, 0, readSize);
                        }
                    }
                }

                InstallUpdate(updateDialog, updateFile);
            }
        }

        private static void DoUpdateWithSingleThread(UpdaterWindow updateDialog, string downloadUrl, string updateFile)
        {
            Thread worker = new Thread(() => DoUpdateWithSingleThreadWorker(updateDialog, downloadUrl, updateFile));
            worker.Name = "Updater.SingleThreadWorker";
            worker.Start();
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string path, uint mode);

        private static void SetUnixPermissions()
        {
            string ryuBin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ryujinx");

            if (!OperatingSystem.IsWindows())
            {
                chmod(ryuBin, 493);
            }
        }

        private static async void InstallUpdate(UpdaterWindow updateDialog, string updateFile)
        {
            // Extract Update
            updateDialog.MainText.Text = LocaleManager.Instance["UpdaterExtracting"];
            updateDialog.ProgressBar.Value = 0;

            if (OperatingSystem.IsLinux())
            {
                using (Stream inStream = File.OpenRead(updateFile))
                using (Stream gzipStream = new GZipInputStream(inStream))
                using (TarInputStream tarStream = new TarInputStream(gzipStream, Encoding.ASCII))
                {
                    updateDialog.ProgressBar.Maximum = inStream.Length;

                    await Task.Run(() =>
                    {
                        TarEntry tarEntry;
                        while ((tarEntry = tarStream.GetNextEntry()) != null)
                        {
                            if (tarEntry.IsDirectory) continue;

                            string outPath = Path.Combine(UpdateDir, tarEntry.Name);

                            Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                            using (FileStream outStream = File.OpenWrite(outPath))
                            {
                                tarStream.CopyEntryContents(outStream);
                            }

                            File.SetLastWriteTime(outPath, DateTime.SpecifyKind(tarEntry.ModTime, DateTimeKind.Utc));

                            TarEntry entry = tarEntry;

                            Dispatcher.UIThread.Post(() =>
                            {
                                updateDialog.ProgressBar.Value += entry.Size;
                            });
                        }
                    });

                    updateDialog.ProgressBar.Value = inStream.Length;
                }
            }
            else
            {
                using (Stream inStream = File.OpenRead(updateFile))
                using (ZipFile zipFile = new ZipFile(inStream))
                {
                    updateDialog.ProgressBar.Maximum = zipFile.Count;

                    await Task.Run(() =>
                    {
                        foreach (ZipEntry zipEntry in zipFile)
                        {
                            if (zipEntry.IsDirectory) continue;

                            string outPath = Path.Combine(UpdateDir, zipEntry.Name);

                            Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                            using (Stream zipStream = zipFile.GetInputStream(zipEntry))
                            using (FileStream outStream = File.OpenWrite(outPath))
                            {
                                zipStream.CopyTo(outStream);
                            }

                            File.SetLastWriteTime(outPath, DateTime.SpecifyKind(zipEntry.DateTime, DateTimeKind.Utc));

                            Dispatcher.UIThread.Post(() =>
                            {
                                updateDialog.ProgressBar.Value++;
                            });
                        }
                    });
                }
            }

            // Delete downloaded zip
            File.Delete(updateFile);

            List<string> allFiles = EnumerateFilesToDelete().ToList();

            updateDialog.MainText.Text = LocaleManager.Instance["UpdaterRenaming"];
            updateDialog.ProgressBar.Value = 0;
            updateDialog.ProgressBar.Maximum = allFiles.Count;

            // Replace old files
            await Task.Run(() =>
            {
                foreach (string file in allFiles)
                {
                    try
                    {
                        File.Move(file, file + ".ryuold");

                        Dispatcher.UIThread.Post(() =>
                        {
                            updateDialog.ProgressBar.Value++;
                        });
                    }
                    catch
                    {
                        Logger.Warning?.Print(LogClass.Application, string.Format(LocaleManager.Instance["UpdaterRenameFailed"], file));
                    }
                }

                Dispatcher.UIThread.Post(() =>
                {
                    updateDialog.MainText.Text = LocaleManager.Instance["UpdaterAddingFiles"];
                    updateDialog.ProgressBar.Value = 0;
                    updateDialog.ProgressBar.Maximum = Directory.GetFiles(UpdatePublishDir, "*", SearchOption.AllDirectories).Length;
                });

                MoveAllFilesOver(UpdatePublishDir, HomeDir, updateDialog);
            });

            Directory.Delete(UpdateDir, true);

            SetUnixPermissions();

            updateDialog.MainText.Text = LocaleManager.Instance["DialogUpdaterCompleteMessage"];
            updateDialog.SecondaryText.Text = LocaleManager.Instance["DialogUpdaterRestartMessage"];

            updateDialog.ProgressBar.IsVisible = false;
            updateDialog.ButtonBox.IsVisible = true;
        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        public static bool CanUpdate(bool showWarnings, StyleableWindow parent)
        {
#if !DISABLE_UPDATER
            if (RuntimeInformation.OSArchitecture != Architecture.X64)
            {
                if (showWarnings)
                {
                    ContentDialogHelper.CreateWarningDialog(LocaleManager.Instance["DialogUpdaterArchNotSupportedMessage"],
                        LocaleManager.Instance["DialogUpdaterArchNotSupportedSubMessage"]);
                }

                return false;
            }

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                if (showWarnings)
                {
                    ContentDialogHelper.CreateWarningDialog(LocaleManager.Instance["DialogUpdaterNoInternetMessage"],
                        LocaleManager.Instance["DialogUpdaterNoInternetSubMessage"]);
                }

                return false;
            }

            if (Program.Version.Contains("dirty") || !ReleaseInformations.IsValid())
            {
                if (showWarnings)
                {
                    ContentDialogHelper.CreateWarningDialog(LocaleManager.Instance["DialogUpdaterDirtyBuildMessage"],
                        LocaleManager.Instance["DialogUpdaterDirtyBuildSubMessage"]);
                }

                return false;
            }

            return true;
#else
            if (showWarnings)
            {
                if (ReleaseInformations.IsFlatHubBuild())
                {
                    ContentDialogHelper.CreateWarningDialog(LocaleManager.Instance["UpdaterDisabledWarningTitle"], LocaleManager.Instance["DialogUpdaterFlatpakNotSupportedMessage"]);
                }
                else
                {
                    ContentDialogHelper.CreateWarningDialog(LocaleManager.Instance["UpdaterDisabledWarningTitle"], LocaleManager.Instance["DialogUpdaterDirtyBuildSubMessage"]);
                }
            }

            return false;
#endif
        }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        // NOTE: This method should always reflect the latest build layout.s
        private static IEnumerable<string> EnumerateFilesToDelete()
        {
            var files = Directory.EnumerateFiles(HomeDir); // All files directly in base dir.

            if (OperatingSystem.IsWindows())
            {
                foreach (string dir in WindowsDependencyDirs)
                {
                    string dirPath = Path.Combine(HomeDir, dir);
                    if (Directory.Exists(dirPath))
                    {
                        files = files.Concat(Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories));
                    }
                }
            }

            return files;
        }

        private static void MoveAllFilesOver(string root, string dest, UpdaterWindow dialog)
        {
            foreach (string directory in Directory.GetDirectories(root))
            {
                string dirName = Path.GetFileName(directory);

                if (!Directory.Exists(Path.Combine(dest, dirName)))
                {
                    Directory.CreateDirectory(Path.Combine(dest, dirName));
                }

                MoveAllFilesOver(directory, Path.Combine(dest, dirName), dialog);
            }

            foreach (string file in Directory.GetFiles(root))
            {
                File.Move(file, Path.Combine(dest, Path.GetFileName(file)), true);

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    dialog.ProgressBar.Value++;
                });
            }
        }

        public static void CleanupUpdate()
        {
            foreach (string file in Directory.GetFiles(HomeDir, "*.ryuold", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }
    }
}
