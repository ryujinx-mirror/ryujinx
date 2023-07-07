using Avalonia.Collections;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Models;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.Ui.App.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Ryujinx.Ava.UI.Windows
{
    public partial class CheatWindow : StyleableWindow
    {
        private readonly string _enabledCheatsPath;
        public bool NoCheatsFound { get; }

        private AvaloniaList<CheatsList> LoadedCheats { get; }

        public string Heading { get; }
        public string BuildId { get; }

        public CheatWindow()
        {
            DataContext = this;

            InitializeComponent();

            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance[LocaleKeys.CheatWindowTitle];
        }

        public CheatWindow(VirtualFileSystem virtualFileSystem, string titleId, string titleName, string titlePath)
        {
            LoadedCheats = new AvaloniaList<CheatsList>();

            Heading = LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.CheatWindowHeading, titleName, titleId.ToUpper());
            BuildId = ApplicationData.GetApplicationBuildId(virtualFileSystem, titlePath);

            InitializeComponent();

            string modsBasePath = ModLoader.GetModsBasePath();
            string titleModsPath = ModLoader.GetTitleDir(modsBasePath, titleId);
            ulong titleIdValue = ulong.Parse(titleId, NumberStyles.HexNumber);

            _enabledCheatsPath = Path.Combine(titleModsPath, "cheats", "enabled.txt");

            string[] enabled = Array.Empty<string>();

            if (File.Exists(_enabledCheatsPath))
            {
                enabled = File.ReadAllLines(_enabledCheatsPath);
            }

            int cheatAdded = 0;

            var mods = new ModLoader.ModCache();

            ModLoader.QueryContentsDir(mods, new DirectoryInfo(Path.Combine(modsBasePath, "contents")), titleIdValue);

            string currentCheatFile = string.Empty;
            string buildId = string.Empty;

            CheatsList currentGroup = null;

            foreach (var cheat in mods.Cheats)
            {
                if (cheat.Path.FullName != currentCheatFile)
                {
                    currentCheatFile = cheat.Path.FullName;
                    string parentPath = currentCheatFile.Replace(titleModsPath, "");

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

            Title = $"Ryujinx {Program.Version} - " + LocaleManager.Instance[LocaleKeys.CheatWindowTitle];
        }

        public void Save()
        {
            if (NoCheatsFound)
            {
                return;
            }

            List<string> enabledCheats = new();

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
