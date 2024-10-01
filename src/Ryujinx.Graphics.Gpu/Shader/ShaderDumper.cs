using System.IO;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Shader dumper, writes binary shader code to disk.
    /// </summary>
    class ShaderDumper
    {
        private string _runtimeDir;
        private string _dumpPath;

        /// <summary>
        /// Current index of the shader dump binary file.
        /// This is incremented after each save, in order to give unique names to the files.
        /// </summary>
        public int CurrentDumpIndex { get; private set; }

        /// <summary>
        /// Creates a new instance of the shader dumper.
        /// </summary>
        public ShaderDumper()
        {
            CurrentDumpIndex = 1;
        }

        /// <summary>
        /// Dumps shader code to disk.
        /// </summary>
        /// <param name="code">Code to be dumped</param>
        /// <param name="compute">True for compute shader code, false for graphics shader code</param>
        /// <returns>Paths where the shader code was dumped</returns>
        public ShaderDumpPaths Dump(byte[] code, bool compute)
        {
            _dumpPath = GraphicsConfig.ShadersDumpPath;

            if (string.IsNullOrWhiteSpace(_dumpPath))
            {
                return default;
            }

            string fileName = "Shader" + CurrentDumpIndex.ToString("d4") + ".bin";

            string fullPath = Path.Combine(FullDir(), fileName);
            string codePath = Path.Combine(CodeDir(), fileName);

            CurrentDumpIndex++;

            using MemoryStream stream = new(code);
            BinaryReader codeReader = new(stream);

            using FileStream fullFile = File.Create(fullPath);
            using FileStream codeFile = File.Create(codePath);
            BinaryWriter fullWriter = new(fullFile);
            BinaryWriter codeWriter = new(codeFile);

            int headerSize = compute ? 0 : 0x50;

            fullWriter.Write(codeReader.ReadBytes(headerSize));

            byte[] temp = codeReader.ReadBytes(code.Length - headerSize);

            fullWriter.Write(temp);
            codeWriter.Write(temp);

            // Align to meet nvdisasm requirements.
            while (codeFile.Length % 0x20 != 0)
            {
                codeWriter.Write(0);
            }

            return new ShaderDumpPaths(fullPath, codePath);
        }

        /// <summary>
        /// Returns the output directory for shader code with header.
        /// </summary>
        /// <returns>Directory path</returns>
        private string FullDir()
        {
            return CreateAndReturn(Path.Combine(DumpDir(), "Full"));
        }

        /// <summary>
        /// Returns the output directory for shader code without header.
        /// </summary>
        /// <returns>Directory path</returns>
        private string CodeDir()
        {
            return CreateAndReturn(Path.Combine(DumpDir(), "Code"));
        }

        /// <summary>
        /// Returns the full output directory for the current shader dump.
        /// </summary>
        /// <returns>Directory path</returns>
        private string DumpDir()
        {
            if (string.IsNullOrEmpty(_runtimeDir))
            {
                int index = 1;

                do
                {
                    _runtimeDir = Path.Combine(_dumpPath, "Dumps" + index.ToString("d2"));

                    index++;
                }
                while (Directory.Exists(_runtimeDir));

                Directory.CreateDirectory(_runtimeDir);
            }

            return _runtimeDir;
        }

        /// <summary>
        /// Creates a new specified directory if needed.
        /// </summary>
        /// <param name="dir">The directory to create</param>
        /// <returns>The same directory passed to the method</returns>
        private static string CreateAndReturn(string dir)
        {
            Directory.CreateDirectory(dir);

            return dir;
        }
    }
}
