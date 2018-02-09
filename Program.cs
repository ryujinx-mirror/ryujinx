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

                        Console.Title += " - Cart (with RomFS) - " + args[0];
                        Ns.Os.LoadCart(args[0], RomFsFiles[0]);
                    }
                    else
                    {
                        Logging.Info("Loading as cart WITHOUT RomFS.");

                        Console.Title += " - Cart (without RomFS) - " + args[0];
                        Ns.Os.LoadCart(args[0]);
                    }
                }
                else if (File.Exists(args[0]))
                {
                    Logging.Info("Loading as homebrew.");

                    Console.Title += " - Homebrew - " + args[0];
                    Ns.Os.LoadProgram(args[0]);
                }
            }
            else
            {
                Logging.Error("Please specify the folder with the NSOs/IStorage or a NSO/NRO.");
            }

            using (GLScreen Screen = new GLScreen(Ns, Renderer))
            {
                Screen.Run(60.0);
            }

            Ns.Os.StopAllProcesses();

            Ns.Dispose();
        }
    }
}
