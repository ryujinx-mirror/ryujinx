using Avalonia;
using Avalonia.Collections;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Ryujinx.Ava.Ui.Windows
{
    public partial class CheatWindow : StyleableWindow
    {
        private readonly string _enabledCheatsPath;
        public bool NoCheatsFound { get; }

        private AvaloniaList<CheatsList> LoadedCheats { get; }

        public string Heading { get; }

        public CheatWindow()
        {
            DataContext = this;

            InitializeComponent();

            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["CheatWindowTitle"];
        }

        public CheatWindow(VirtualFileSystem virtualFileSystem, string titleId, string titleName)
        {
            LoadedCheats = new AvaloniaList<CheatsList>();

            Heading = string.Format(LocaleManager.Instance["CheatWindowHeading"], titleName, titleId.ToUpper());

            InitializeComponent();

            string modsBasePath = virtualFileSystem.ModLoader.GetModsBasePath();
            string titleModsPath = virtualFileSystem.ModLoader.GetTitleDir(modsBasePath, titleId);
            ulong titleIdValue = ulong.Parse(titleId, System.Globalization.NumberStyles.HexNumber);

            _enabledCheatsPath = Path.Combine(titleModsPath, "cheats", "enabled.txt");

            string[] enabled = { };

            if (File.Exists(_enabledCheatsPath))
            {
                enabled = File.ReadAllLines(_enabledCheatsPath);
            }

            int cheatAdded = 0;

            var mods = new ModLoader.ModCache();

            ModLoader.QueryContentsDir(mods, new DirectoryInfo(Path.Combine(modsBasePath, "contents")), titleIdValue);

            string currentCheatFile = string.Empty;
            string buildId = string.Empty;
            string parentPath = string.Empty;

            CheatsList currentGroup = null;

            foreach (var cheat in mods.Cheats)
            {
                if (cheat.Path.FullName != currentCheatFile)
                {
                    currentCheatFile = cheat.Path.FullName;
                    parentPath = currentCheatFile.Replace(titleModsPath, "");

                    buildId = Path.GetFileNameWithoutExtension(currentCheatFile).ToUpper();
                    currentGroup = new CheatsList(buildId, parentPath);

                    LoadedCheats.Add(currentGroup);
                }

                var model = new CheatModel(cheat.Name, buildId, enabled.Contains($"{buildId}-{cheat.Name}"));
                currentGroup?.Add(model);

                cheatAdded++;
            }

            if (cheatAdded == 0)
            {
                NoCheatsFound = true;
            }

            DataContext = this;
            
            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance["CheatWindowTitle"];
        }

        public void Save()
        {
            if (NoCheatsFound)
            {
                return;
            }

            List<string> enabledCheats = new List<string>();

            foreach (var cheats in LoadedCheats)
            {
                foreach (var cheat in cheats)
                {
                    if (cheat.IsEnabled)
                    {
                        enabledCheats.Add(cheat.BuildIdKey);
                    }
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_enabledCheatsPath));

            File.WriteAllLines(_enabledCheatsPath, enabledCheats);

            Close();
        }
    }
}