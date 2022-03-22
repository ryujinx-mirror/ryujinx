using Gtk;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ncm;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.HLE.FileSystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Image = SixLabors.ImageSharp.Image;

namespace Ryujinx.Ui.Windows
{
    public class AvatarWindow : Window
    {
        public byte[] SelectedProfileImage;
        public bool   NewUser;

        private static Dictionary<string, byte[]> _avatarDict = new Dictionary<string, byte[]>();

        private ListStore _listStore;
        private IconView  _iconView;
        private Button    _setBackgroungColorButton;
        private Gdk.RGBA  _backgroundColor;

        public AvatarWindow() : base($"Ryujinx {Program.Version} - Manage Accounts - Avatar")
        {
            Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.Resources.Logo_Ryujinx.png");

            CanFocus  = false;
            Resizable = false;
            Modal     = true;
            TypeHint  = Gdk.WindowTypeHint.Dialog;

            SetDefaultSize(740, 400);
            SetPosition(WindowPosition.Center);

            VBox vbox = new VBox(false, 0);
            Add(vbox);

            ScrolledWindow scrolledWindow = new ScrolledWindow
            {
                ShadowType = ShadowType.EtchedIn
            };
            scrolledWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            HBox hbox = new HBox(false, 0);

            Button chooseButton = new Button()
            {
                Label           = "Choose",
                CanFocus        = true,
                ReceivesDefault = true
            };
            chooseButton.Clicked += ChooseButton_Pressed;

            _setBackgroungColorButton = new Button()
            {
                Label    = "Set Background Color",
                CanFocus = true
            };
            _setBackgroungColorButton.Clicked += SetBackgroungColorButton_Pressed;

            _backgroundColor.Red   = 1;
            _backgroundColor.Green = 1;
            _backgroundColor.Blue  = 1;
            _backgroundColor.Alpha = 1;

            Button closeButton = new Button()
            {
                Label           = "Close",
                CanFocus        = true
            };
            closeButton.Clicked += CloseButton_Pressed;

            vbox.PackStart(scrolledWindow,            true,  true,  0);
            hbox.PackStart(chooseButton,              true,  true,  0);
            hbox.PackStart(_setBackgroungColorButton, true,  true,  0);
            hbox.PackStart(closeButton,               true,  true,  0);
            vbox.PackStart(hbox,                      false, false, 0);

            _listStore = new ListStore(typeof(string), typeof(Gdk.Pixbuf));
            _listStore.SetSortColumnId(0, SortType.Ascending);

            _iconView              = new IconView(_listStore);
            _iconView.ItemWidth    = 64;
            _iconView.ItemPadding  = 10;
            _iconView.PixbufColumn = 1;

            _iconView.SelectionChanged += IconView_SelectionChanged;

            scrolledWindow.Add(_iconView);

            _iconView.GrabFocus();

            ProcessAvatars();

            ShowAll();
        }

        public static void PreloadAvatars(ContentManager contentManager, VirtualFileSystem virtualFileSystem)
        {
            if (_avatarDict.Count > 0)
            {
                return;
            }

            string contentPath = contentManager.GetInstalledContentPath(0x010000000000080A, StorageId.BuiltInSystem, NcaContentType.Data);
            string avatarPath  = virtualFileSystem.SwitchPathToSystemPath(contentPath);

            if (!string.IsNullOrWhiteSpace(avatarPath))
            {
                using (IStorage ncaFileStream = new LocalStorage(avatarPath, FileAccess.Read, FileMode.Open))
                {
                    Nca         nca   = new Nca(virtualFileSystem.KeySet, ncaFileStream);
                    IFileSystem romfs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

                    foreach (var item in romfs.EnumerateEntries())
                    {
                        // TODO: Parse DatabaseInfo.bin and table.bin files for more accuracy.

                        if (item.Type == DirectoryEntryType.File && item.FullPath.Contains("chara") && item.FullPath.Contains("szs"))
                        {
                            using var file = new UniqueRef<IFile>();

                            romfs.OpenFile(ref file.Ref(), ("/" + item.FullPath).ToU8Span(), OpenMode.Read).ThrowIfFailure();

                            using (MemoryStream stream    = new MemoryStream())
                            using (MemoryStream streamPng = new MemoryStream())
                            {
                                file.Get.AsStream().CopyTo(stream);

                                stream.Position = 0;

                                Image avatarImage = Image.LoadPixelData<Rgba32>(DecompressYaz0(stream), 256, 256);

                                avatarImage.SaveAsPng(streamPng);

                                _avatarDict.Add(item.FullPath, streamPng.ToArray());
                            }
                        }
                    }
                }
            }
        }

        private void ProcessAvatars()
        {
            _listStore.Clear();

            foreach (var avatar in _avatarDict)
            {
                _listStore.AppendValues(avatar.Key, new Gdk.Pixbuf(ProcessImage(avatar.Value), 96, 96));
            }

            _iconView.SelectPath(new TreePath(new int[] { 0 }));
        }

        private byte[] ProcessImage(byte[] data)
        {
            using (MemoryStream streamJpg = new MemoryStream())
            {
                Image avatarImage = Image.Load(data, new PngDecoder());

                avatarImage.Mutate(x => x.BackgroundColor(new Rgba32((byte)(_backgroundColor.Red   * 255),
                                                                     (byte)(_backgroundColor.Green * 255),
                                                                     (byte)(_backgroundColor.Blue  * 255),
                                                                     (byte)(_backgroundColor.Alpha * 255))));
                avatarImage.SaveAsJpeg(streamJpg);

                return streamJpg.ToArray();
            }
        }

        private void CloseButton_Pressed(object sender, EventArgs e)
        {
            SelectedProfileImage = null;

            Close();
        }

        private void IconView_SelectionChanged(object sender, EventArgs e)
        {
            if (_iconView.SelectedItems.Length > 0)
            {
                _listStore.GetIter(out TreeIter iter, _iconView.SelectedItems[0]);

                SelectedProfileImage = ProcessImage(_avatarDict[(string)_listStore.GetValue(iter, 0)]);
            }
        }

        private void SetBackgroungColorButton_Pressed(object sender, EventArgs e)
        {
            using (ColorChooserDialog colorChooserDialog = new ColorChooserDialog("Set Background Color", this))
            {
                colorChooserDialog.UseAlpha = false;
                colorChooserDialog.Rgba     = _backgroundColor;
                
                if (colorChooserDialog.Run() == (int)ResponseType.Ok)
                {
                    _backgroundColor = colorChooserDialog.Rgba;

                    ProcessAvatars();
                }

                colorChooserDialog.Hide();
            }
        }

        private void ChooseButton_Pressed(object sender, EventArgs e)
        {
            Close();
        }

        private static byte[] DecompressYaz0(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.ReadInt32(); // Magic
                
                uint decodedLength = BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());

                reader.ReadInt64(); // Padding

                byte[] input = new byte[stream.Length - stream.Position];
                stream.Read(input, 0, input.Length);

                long inputOffset = 0;

                byte[] output       = new byte[decodedLength];
                long   outputOffset = 0;

                ushort mask   = 0;
                byte   header = 0;

                while (outputOffset < decodedLength)
                {
                    if ((mask >>= 1) == 0)
                    {
                        header = input[inputOffset++];
                        mask   = 0x80;
                    }

                    if ((header & mask) > 0)
                    {
                        if (outputOffset == output.Length)
                        {
                            break;
                        }

                        output[outputOffset++] = input[inputOffset++];
                    }
                    else
                    {
                        byte byte1 = input[inputOffset++];
                        byte byte2 = input[inputOffset++];

                        int dist     = ((byte1 & 0xF) << 8) | byte2;
                        int position = (int)outputOffset - (dist + 1);

                        int length = byte1 >> 4;
                        if (length == 0)
                        {
                            length = input[inputOffset++] + 0x12;
                        }
                        else
                        {
                            length += 2;
                        }

                        while (length-- > 0)
                        {
                            output[outputOffset++] = output[position++];
                        }
                    }
                }

                return output;
            }
        }
    }
}