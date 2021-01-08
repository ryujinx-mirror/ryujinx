using Gtk;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Ryujinx.Common.Logging;
using Ryujinx.Ui;
using Ryujinx.Ui.Widgets;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ryujinx.Modules
{
    public static class Updater
    {
        internal static bool Running;

        private static readonly string HomeDir          = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string UpdateDir        = Path.Combine(Path.GetTempPath(), "Ryujinx", "update");
        private static readonly string UpdatePublishDir = Path.Combine(UpdateDir, "publish");

        private static string _jobId;
        private static string _buildVer;
        private static string _platformExt;
        private static string _buildUrl;
        
        private const string AppveyorApiUrl = "https://ci.appveyor.com/api";

        public static async Task BeginParse(MainWindow mainWindow, bool showVersionUpToDate)
        {
            if (Running) return;

            Running = true;
            mainWindow.UpdateMenuItem.Sensitive = false;

            // Detect current platform
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _platformExt = "osx_x64.zip";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _platformExt = "win_x64.zip";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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
                GtkDialog.CreateWarningDialog("Failed to convert the current Ryujinx version.", "Cancelling Update!");
                Logger.Error?.Print(LogClass.Application, "Failed to convert the current Ryujinx version!");

                return;
            }

            // Get latest version number from Appveyor
            try
            {
                using (WebClient jsonClient = new WebClient())
                {
                    string  fetchedJson = await jsonClient.DownloadStringTaskAsync($"{AppveyorApiUrl}/projects/gdkchan/ryujinx/branch/master");
                    JObject jsonRoot    = JObject.Parse(fetchedJson);
                    JToken  buildToken  = jsonRoot["build"];

                    _jobId    = (string)buildToken["jobs"][0]["jobId"];
                    _buildVer = (string)buildToken["version"];
                    _buildUrl = $"{AppveyorApiUrl}/buildjobs/{_jobId}/artifacts/ryujinx-{_buildVer}-{_platformExt}";

                    // If build not done, assume no new update are availaible.
                    if ((string)buildToken["jobs"][0]["status"] != "success")
                    {
                        if (showVersionUpToDate)
                        {
                            GtkDialog.CreateUpdaterInfoDialog("You are already using the most updated version of Ryujinx!", "");
                        }

                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error?.Print(LogClass.Application, exception.Message);
                GtkDialog.CreateErrorDialog("An error has occurred when trying to get release information from AppVeyor.");

                return;
            }

            try
            {
                newVersion = Version.Parse(_buildVer);
            }
            catch
            {
                GtkDialog.CreateWarningDialog("Failed to convert the received Ryujinx version from AppVeyor.", "Cancelling Update!");
                Logger.Error?.Print(LogClass.Application, "Failed to convert the received Ryujinx version from AppVeyor!");

                return;
            }

            if (newVersion <= currentVersion)
            {
                if (showVersionUpToDate)
                {
                    GtkDialog.CreateUpdaterInfoDialog("You are already using the most updated version of Ryujinx!", "");
                }

                Running = false;
                mainWindow.UpdateMenuItem.Sensitive = true;

                return;
            }

            // Show a message asking the user if they want to update
            UpdateDialog updateDialog = new UpdateDialog(mainWindow, newVersion, _buildUrl);
            updateDialog.Show();
        }

        public static async Task UpdateRyujinx(UpdateDialog updateDialog, string downloadUrl)
        {
            // Empty update dir, although it shouldn't ever have anything inside it
            if (Directory.Exists(UpdateDir))
            {
                Directory.Delete(UpdateDir, true);
            }

            Directory.CreateDirectory(UpdateDir);

            string updateFile = Path.Combine(UpdateDir, "update.bin");

            // Download the update .zip
            updateDialog.MainText.Text        = "Downloading Update...";
            updateDialog.ProgressBar.Value    = 0;
            updateDialog.ProgressBar.MaxValue = 100;

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (_, args) =>
                {
                    updateDialog.ProgressBar.Value = args.ProgressPercentage;
                };

                await client.DownloadFileTaskAsync(downloadUrl, updateFile);
            }

            // Extract Update
            updateDialog.MainText.Text     = "Extracting Update...";
            updateDialog.ProgressBar.Value = 0;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using (Stream         inStream   = File.OpenRead(updateFile))
                using (Stream         gzipStream = new GZipInputStream(inStream))
                using (TarInputStream tarStream  = new TarInputStream(gzipStream, Encoding.ASCII))
                {
                    updateDialog.ProgressBar.MaxValue = inStream.Length;

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

                            Application.Invoke(delegate
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
                using (Stream  inStream = File.OpenRead(updateFile))
                using (ZipFile zipFile  = new ZipFile(inStream))
                {
                    updateDialog.ProgressBar.MaxValue = zipFile.Count;

                    await Task.Run(() =>
                    {
                        foreach (ZipEntry zipEntry in zipFile)
                        {
                            if (zipEntry.IsDirectory) continue;

                            string outPath = Path.Combine(UpdateDir, zipEntry.Name);

                            Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                            using (Stream     zipStream = zipFile.GetInputStream(zipEntry))
                            using (FileStream outStream = File.OpenWrite(outPath))
                            {
                                zipStream.CopyTo(outStream);
                            }

                            File.SetLastWriteTime(outPath, DateTime.SpecifyKind(zipEntry.DateTime, DateTimeKind.Utc));

                            Application.Invoke(delegate
                            {
                                updateDialog.ProgressBar.Value++;
                            });
                        }
                    });
                }
            }

            // Delete downloaded zip
            File.Delete(updateFile);

            string[] allFiles = Directory.GetFiles(HomeDir, "*", SearchOption.AllDirectories);

            updateDialog.MainText.Text        = "Renaming Old Files...";
            updateDialog.ProgressBar.Value    = 0;
            updateDialog.ProgressBar.MaxValue = allFiles.Length;

            // Replace old files
            await Task.Run(() =>
            {
                foreach (string file in allFiles)
                {
                    if (!Path.GetExtension(file).Equals(".log"))
                    {
                        try
                        {
                            File.Move(file, file + ".ryuold");

                            Application.Invoke(delegate
                            {
                                updateDialog.ProgressBar.Value++;
                            });
                        }
                        catch
                        {
                            Logger.Warning?.Print(LogClass.Application, "Updater wasn't able to rename file: " + file);
                        }
                    }
                }

                Application.Invoke(delegate
                {
                    updateDialog.MainText.Text        = "Adding New Files...";
                    updateDialog.ProgressBar.Value    = 0;
                    updateDialog.ProgressBar.MaxValue = Directory.GetFiles(UpdatePublishDir, "*", SearchOption.AllDirectories).Length;
                });

                MoveAllFilesOver(UpdatePublishDir, HomeDir, updateDialog);
            });

            Directory.Delete(UpdateDir, true);

            updateDialog.MainText.Text      = "Update Complete!";
            updateDialog.SecondaryText.Text = "Do you want to restart Ryujinx now?";
            updateDialog.Modal              = true;

            updateDialog.ProgressBar.Hide();
            updateDialog.YesButton.Show();
            updateDialog.NoButton.Show();
        }

        public static bool CanUpdate(bool showWarnings)
        {
            if (RuntimeInformation.OSArchitecture != Architecture.X64)
            {
                if (showWarnings)
                {
                    GtkDialog.CreateWarningDialog("You are not running a supported system architecture!", "(Only x64 systems are supported!)");
                }

                return false;
            }

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                if (showWarnings)
                {
                    GtkDialog.CreateWarningDialog("You are not connected to the Internet!", "Please verify that you have a working Internet connection!");
                }

                return false;
            }

            if (Program.Version.Contains("dirty"))
            {
                if (showWarnings)
                {
                    GtkDialog.CreateWarningDialog("You Cannot update a Dirty build of Ryujinx!", "Please download Ryujinx at https://ryujinx.org/ if you are looking for a supported version.");
                }

                return false;
            }

            return true;
        }

        private static void MoveAllFilesOver(string root, string dest, UpdateDialog dialog)
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

                Application.Invoke(delegate
                {
                    dialog.ProgressBar.Value++;
                });
            }
        }

        public static void CleanupUpdate()
        {
            foreach (string file in Directory.GetFiles(HomeDir, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file).EndsWith(".ryuold"))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
