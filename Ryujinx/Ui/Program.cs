using Ryujinx.Audio;
using Ryujinx.Audio.OpenAL;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Gal.OpenGL;
using Ryujinx.HLE;
using System;
using System.IO;

namespace Ryujinx
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Ryujinx Console";

            IGalRenderer Renderer = new OGLRenderer();

            IAalOutput AudioOut = new OpenALAudioOut();

            Switch Device = new Switch(Renderer, AudioOut);

            Config.Read(Device);

            Device.Log.Updated += ConsoleLog.Log;

            if (args.Length == 1)
            {
                if (Directory.Exists(args[0]))
                {
                    string[] RomFsFiles = Directory.GetFiles(args[0], "*.istorage");

                    if (RomFsFiles.Length == 0)
                    {
                        RomFsFiles = Directory.GetFiles(args[0], "*.romfs");
                    }

                    if (RomFsFiles.Length > 0)
                    {
                        Console.WriteLine("Loading as cart with RomFS.");

                        Device.LoadCart(args[0], RomFsFiles[0]);
                    }
                    else
                    {
                        Console.WriteLine("Loading as cart WITHOUT RomFS.");

                        Device.LoadCart(args[0]);
                    }
                }
                else if (File.Exists(args[0]))
                {
                    switch (Path.GetExtension(args[0]).ToLowerInvariant())
                    {
                        case ".xci":
                            Console.WriteLine("Loading as XCI.");
                            Device.LoadXci(args[0]);
                            break;
                        case ".nca":
                            Console.WriteLine("Loading as NCA.");
                            Device.LoadNca(args[0]);
                            break;
                        case ".nsp":
                            Console.WriteLine("Loading as NSP.");
                            Device.LoadNsp(args[0]);
                            break;
                        default:
                            Console.WriteLine("Loading as homebrew.");
                            Device.LoadProgram(args[0]);
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Please specify the folder with the NSOs/IStorage or a NSO/NRO.");
            }

            using (GLScreen Screen = new GLScreen(Device, Renderer))
            {
                Screen.MainLoop();

                Device.Dispose();
            }

            AudioOut.Dispose();
        }
    }
}
