using Gal;
using Gal.OpenGL;
using System;
using System.IO;

namespace Ryujinx
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.Read();

            Console.Title = "Ryujinx Console";

            IGalRenderer Renderer = new OpenGLRenderer();

            Switch Ns = new Switch(Renderer);

            if (args.Length == 1)
            {
                if (Directory.Exists(args[0]))
                {
                    string[] RomFsFiles = Directory.GetFiles(args[0], "*.istorage");

                    if (RomFsFiles.Length > 0)
                    {
                        Logging.Info("Loading as cart with RomFS.");

                        Ns.Os.LoadCart(args[0], RomFsFiles[0]);
                    }
                    else
                    {
                        Logging.Info("Loading as cart WITHOUT RomFS.");

                        Ns.Os.LoadCart(args[0]);
                    }
                }
                else if (File.Exists(args[0]))
                {
                    Logging.Info("Loading as homebrew.");

                    Ns.Os.LoadProgram(args[0]);
                }
            }
            else
            {
                Logging.Error("Please specify the folder with the NSOs/IStorage or a NSO/NRO.");
            }

            using (GLScreen Screen = new GLScreen(Ns, Renderer))
            {
                Ns.Finish += (Sender, Args) => {
                    Screen.Exit();
                };

                Screen.Run(60.0);
            }

            Ns.Os.FinalizeAllProcesses();

            Ns.Dispose();
        }
    }
}
