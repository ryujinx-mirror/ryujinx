using Gtk;
using LibHac;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Switch = Ryujinx.HLE.Switch;

namespace Ryujinx.Ui
{
    internal class Migration
    {
        private Switch Device { get; }

        public Migration(Switch device)
        {
            Device = device;
        }

        public static bool PromptIfMigrationNeededForStartup(Window parentWindow, out bool isMigrationNeeded)
        {
            if (!IsMigrationNeeded())
            {
                isMigrationNeeded = false;

                return true;
            }

            isMigrationNeeded = true;

            int dialogResponse;

            using (MessageDialog dialog = new MessageDialog(parentWindow, DialogFlags.Modal, MessageType.Question,
                ButtonsType.YesNo, "What's this?"))
            {
                dialog.Title = "Data Migration Needed";
                dialog.Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png");
                dialog.Text =
                    "The folder structure of Ryujinx's RyuFs folder has been updated and renamed to \"Ryujinx\". " +
                    "Your RyuFs folder must be copied and migrated to the new \"Ryujinx\" structure. Would you like to do the migration now?\n\n" +
                    "Select \"Yes\" to automatically perform the migration. Your old RyuFs folder will remain as it is.\n\n" +
                    "Selecting \"No\" will exit Ryujinx without changing anything.";

                dialogResponse = dialog.Run();
            }

            return dialogResponse == (int)ResponseType.Yes;
        }

        public static bool DoMigrationForStartup(Window parentWindow, Switch device)
        {
            try
            {
                Migration migration = new Migration(device);
                int saveCount = migration.Migrate();

                using MessageDialog dialogSuccess = new MessageDialog(parentWindow, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, null)
                {
                    Title = "Migration Success",
                    Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png"),
                    Text = $"Data migration was successful. {saveCount} saves were migrated.",
                };

                dialogSuccess.Run();

                return true;
            }
            catch (HorizonResultException ex)
            {
                GtkDialog.CreateErrorDialog(ex.Message);

                return false;
            }
        }

        // Returns the number of saves migrated
        public int Migrate()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string oldBasePath = Path.Combine(appDataPath, "RyuFs");
            string newBasePath = Path.Combine(appDataPath, "Ryujinx");

            string oldSaveDir = Path.Combine(oldBasePath, "nand/user/save");

            CopyRyuFs(oldBasePath, newBasePath);

            SaveImporter importer = new SaveImporter(oldSaveDir, Device.System.FsClient);

            return importer.Import();
        }

        private static void CopyRyuFs(string oldPath, string newPath)
        {
            Directory.CreateDirectory(newPath);

            CopyExcept(oldPath, newPath, "nand", "bis", "sdmc", "sdcard");

            string oldNandPath = Path.Combine(oldPath, "nand");
            string newNandPath = Path.Combine(newPath, "bis");

            CopyExcept(oldNandPath, newNandPath, "system", "user");

            string oldSdPath = Path.Combine(oldPath, "sdmc");
            string newSdPath = Path.Combine(newPath, "sdcard");

            CopyDirectory(oldSdPath, newSdPath);

            string oldSystemPath = Path.Combine(oldNandPath, "system");
            string newSystemPath = Path.Combine(newNandPath, "system");

            CopyExcept(oldSystemPath, newSystemPath, "save");

            string oldUserPath = Path.Combine(oldNandPath, "user");
            string newUserPath = Path.Combine(newNandPath, "user");

            CopyExcept(oldUserPath, newUserPath, "save");
        }

        private static void CopyExcept(string srcPath, string dstPath, params string[] exclude)
        {
            exclude = exclude.Select(x => x.ToLowerInvariant()).ToArray();

            DirectoryInfo srcDir = new DirectoryInfo(srcPath);

            if (!srcDir.Exists)
            {
                return;
            }

            Directory.CreateDirectory(dstPath);

            foreach (DirectoryInfo subDir in srcDir.EnumerateDirectories())
            {
                if (exclude.Contains(subDir.Name.ToLowerInvariant()))
                {
                    continue;
                }

                CopyDirectory(subDir.FullName, Path.Combine(dstPath, subDir.Name));
            }

            foreach (FileInfo file in srcDir.EnumerateFiles())
            {
                file.CopyTo(Path.Combine(dstPath, file.Name));
            }
        }

        private static void CopyDirectory(string srcPath, string dstPath)
        {
            Directory.CreateDirectory(dstPath);

            DirectoryInfo srcDir = new DirectoryInfo(srcPath);

            if (!srcDir.Exists)
            {
                return;
            }

            Directory.CreateDirectory(dstPath);

            foreach (DirectoryInfo subDir in srcDir.EnumerateDirectories())
            {
                CopyDirectory(subDir.FullName, Path.Combine(dstPath, subDir.Name));
            }

            foreach (FileInfo file in srcDir.EnumerateFiles())
            {
                file.CopyTo(Path.Combine(dstPath, file.Name));
            }
        }

        private static bool IsMigrationNeeded()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string oldBasePath = Path.Combine(appDataPath, "RyuFs");
            string newBasePath = Path.Combine(appDataPath, "Ryujinx");

            return Directory.Exists(oldBasePath) && !Directory.Exists(newBasePath);
        }
    }
}
