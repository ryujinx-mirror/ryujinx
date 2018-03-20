using Ryujinx.Audio;
using Ryujinx.Audio.OpenAL;
using Ryujinx.Core;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Gal.OpenGL;
using System;
using System.IO;

namespace Ryujinx
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.Read();

            AOptimizations.DisableMemoryChecks = !Config.EnableMemoryChecks;

            Console.Title = "Ryujinx Console";

            IGalRenderer Renderer = new OpenGLRenderer();

            IAalOutput AudioOut = new OpenALAudioOut();

            Switch Ns = new Switch(Renderer, AudioOut);

            if (args.Length == 1)
            {
                if (Directory.Exists(args[0]))
                {
                    string[] RomFsFiles = Directory.GetFiles(args[0], "*.istorage");

                    if (RomFsFiles.Length > 0)
                    {
                        Logging.Info("Loading as cart with RomFS.");

                        Ns.LoadCart(args[0], RomFsFiles[0]);
                    }
                    else
                    {
                        Logging.Info("Loading as cart WITHOUT RomFS.");

                        Ns.LoadCart(args[0]);
                    }
                }
                else if (File.Exists(args[0]))
                {
                    Logging.Info("Loading as homebrew.");

                    Ns.LoadProgram(args[0]);
                }
            }
            else
            {
                Logging.Error("Please specify the folder with the NSOs/IStorage or a NSO/NRO.");
            }

            using (GLScreen Screen = new GLScreen(Ns, Renderer))
            {
                Ns.Finish += (Sender, Args) =>
                {
                    Screen.Exit();
                };

                Screen.Run(60.0);
            }

            Environment.Exit(0);
        }
    }
}
