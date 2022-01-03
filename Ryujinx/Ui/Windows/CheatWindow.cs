using Gtk;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using GUI        = Gtk.Builder.ObjectAttribute;
using JsonHelper = Ryujinx.Common.Utilities.JsonHelper;

namespace Ryujinx.Ui.Windows
{
    public class CheatWindow : Window
    {
        private readonly string _enabledCheatsPath;
        private readonly bool _noCheatsFound;

#pragma warning disable CS0649, IDE0044
        [GUI] Label    _baseTitleInfoLabel;
        [GUI] TreeView _cheatTreeView;
        [GUI] Button   _saveButton;
#pragma warning restore CS0649, IDE0044

        public CheatWindow(VirtualFileSystem virtualFileSystem, ulong titleId, string titleName) : this(new Builder("Ryujinx.Ui.Windows.CheatWindow.glade"), virtualFileSystem, titleId, titleName) { }

        private CheatWindow(Builder builder, VirtualFileSystem virtualFileSystem, ulong titleId, string titleName) : base(builder.GetObject("_cheatWindow").Handle)
        {
            builder.Autoconnect(this);
            _baseTitleInfoLabel.Text = $"Cheats Available for {titleName} [{titleId:X16}]";

            string modsBasePath  = virtualFileSystem.ModLoader.GetModsBasePath();
            string titleModsPath = virtualFileSystem.ModLoader.GetTitleDir(modsBasePath, titleId.ToString("X16"));

            _enabledCheatsPath = System.IO.Path.Combine(titleModsPath, "cheats", "enabled.txt");

            _cheatTreeView.Model = new TreeStore(typeof(bool), typeof(string), typeof(string), typeof(string));

            CellRendererToggle enableToggle = new CellRendererToggle();
            enableToggle.Toggled += (sender, args) =>
            {
                _cheatTreeView.Model.GetIter(out TreeIter treeIter, new TreePath(args.Path));
                bool newValue = !(bool)_cheatTreeView.Model.GetValue(treeIter, 0);
                _cheatTreeView.Model.SetValue(treeIter, 0, newValue);

                if (_cheatTreeView.Model.IterChildren(out TreeIter childIter, treeIter))
                {
                    do
                    {
                        _cheatTreeView.Model.SetValue(childIter, 0, newValue);
                    }
                    while (_cheatTreeView.Model.IterNext(ref childIter));
                }
            };

            _cheatTreeView.AppendColumn("Enabled", enableToggle, "active", 0);
            _cheatTreeView.AppendColumn("Name", new CellRendererText(), "text", 1);
            _cheatTreeView.AppendColumn("Path", new CellRendererText(), "text", 2);

            var buildIdColumn = _cheatTreeView.AppendColumn("Build Id", new CellRendererText(), "text", 3);
            buildIdColumn.Visible = false;

            string[] enabled = { };

            if (File.Exists(_enabledCheatsPath))
            {
                enabled = File.ReadAllLines(_enabledCheatsPath);
            }

            int cheatAdded = 0;

            var mods = new ModLoader.ModCache();

            ModLoader.QueryContentsDir(mods, new DirectoryInfo(System.IO.Path.Combine(modsBasePath, "contents")), titleId);

            string currentCheatFile = string.Empty;
            string buildId = string.Empty;
            TreeIter parentIter = default;

            foreach (var cheat in mods.Cheats)
            {
                if (cheat.Path.FullName != currentCheatFile)
                {
                    currentCheatFile = cheat.Path.FullName;
                    string parentPath = currentCheatFile.Replace(titleModsPath, "");

                    buildId = System.IO.Path.GetFileNameWithoutExtension(currentCheatFile).ToUpper();
                    parentIter = ((TreeStore)_cheatTreeView.Model).AppendValues(false, buildId, parentPath, "");
                }

                string cleanName = cheat.Name.Substring(1, cheat.Name.Length - 8);
                ((TreeStore)_cheatTreeView.Model).AppendValues(parentIter, enabled.Contains($"{buildId}-{cheat.Name}"), cleanName, "", buildId);

                cheatAdded++;
            }

            if (cheatAdded == 0)
            {
                ((TreeStore)_cheatTreeView.Model).AppendValues(false, "No Cheats Found", "", "");
                _cheatTreeView.GetColumn(0).Visible = false;

                _noCheatsFound = true;

                _saveButton.Visible = false;
            }

            _cheatTreeView.ExpandAll();
        }

        private void SaveButton_Clicked(object sender, EventArgs args)
        {
            if (_noCheatsFound)
            {
                return;
            }

            List<string> enabledCheats = new List<string>();

            if (_cheatTreeView.Model.GetIterFirst(out TreeIter parentIter))
            {
                do
                {
                    if (_cheatTreeView.Model.IterChildren(out TreeIter childIter, parentIter))
                    {
                        do
                        {
                            var enabled = (bool)_cheatTreeView.Model.GetValue(childIter, 0);

                            if (enabled)
                            {
                                var name = _cheatTreeView.Model.GetValue(childIter, 1).ToString();
                                var buildId = _cheatTreeView.Model.GetValue(childIter, 3).ToString();

                                enabledCheats.Add($"{buildId}-<{name} Cheat>");
                            }
                        }
                        while (_cheatTreeView.Model.IterNext(ref childIter));
                    }
                }
                while (_cheatTreeView.Model.IterNext(ref parentIter));
            }

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_enabledCheatsPath));

            File.WriteAllLines(_enabledCheatsPath, enabledCheats);

            Dispose();
        }

        private void CancelButton_Clicked(object sender, EventArgs args)
        {
            Dispose();
        }
    }
}
