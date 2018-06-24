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

            Switch Ns = new Switch(Renderer, AudioOut);

            Config.Read(Ns.Log);

            Ns.Log.Updated += ConsoleLog.PrintLog;

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

                        Ns.LoadCart(args[0], RomFsFiles[0]);
                    }
                    else
                    {
                        Console.WriteLine("Loading as cart WITHOUT RomFS.");

                        Ns.LoadCart(args[0]);
                    }
                }
                else if (File.Exists(args[0]))
                {
                    Console.WriteLine("Loading as homebrew.");

                    Ns.LoadProgram(args[0]);
                }
            }
            else
            {
                Console.WriteLine("Please specify the folder with the NSOs/IStorage or a NSO/NRO.");
            }

            using (GLScreen Screen = new GLScreen(Ns, Renderer))
            {
                Ns.Finish += (Sender, Args) =>
                {
                    Screen.Exit();
                };

                Screen.Run(0.0, 60.0);
            }

            Environment.Exit(0);
        }
    }
}
