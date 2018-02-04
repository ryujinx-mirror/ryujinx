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
            IGalRenderer Renderer = new OpenGLRenderer();

            Switch Ns = new Switch(Renderer);

            if (args.Length == 1)
            {
                if (Directory.Exists(args[0]))
                {
                    string[] RomFsFiles = Directory.GetFiles(args[0], "*.istorage");

                    if (RomFsFiles.Length > 0)
                    {
                        Console.WriteLine("Loading as cart with RomFS.");

                        Ns.Os.LoadCart(args[0], RomFsFiles[0]);
                    }
                    else
                    {
                        Console.WriteLine("Loading as cart WITHOUT RomFS.");

                        Ns.Os.LoadCart(args[0]);
                    }
                }
                else if (File.Exists(args[0]))
                {
                    Console.WriteLine("Loading as homebrew.");

                    Ns.Os.LoadProgram(args[0]);
                }
            }
            else
            {
                Console.WriteLine("Please specify the folder with the NSOs/IStorage or a NSO/NRO.");
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
