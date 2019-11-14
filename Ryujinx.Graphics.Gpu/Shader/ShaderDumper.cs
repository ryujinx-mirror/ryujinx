using Ryujinx.Graphics.Shader.Translation;
using System;
using System.IO;

namespace Ryujinx.Graphics.Gpu.Shader
{
    class ShaderDumper
    {
        private GpuContext _context;

        private string _runtimeDir;
        private string _dumpPath;
        private int    _dumpIndex;

        public int CurrentDumpIndex => _dumpIndex;

        public ShaderDumper(GpuContext context)
        {
            _context = context;

            _dumpIndex = 1;
        }

        public void Dump(Span<byte> code, bool compute, out string fullPath, out string codePath)
        {
            _dumpPath = GraphicsConfig.ShadersDumpPath;

            if (string.IsNullOrWhiteSpace(_dumpPath))
            {
                fullPath = null;
                codePath = null;

                return;
            }

            string fileName = "Shader" + _dumpIndex.ToString("d4") + ".bin";

            fullPath = Path.Combine(FullDir(), fileName);
            codePath = Path.Combine(CodeDir(), fileName);

            _dumpIndex++;

            code = Translator.ExtractCode(code, compute, out int headerSize);

            using (MemoryStream stream = new MemoryStream(code.ToArray()))
            {
                BinaryReader codeReader = new BinaryReader(stream);

                using (FileStream fullFile = File.Create(fullPath))
                using (FileStream codeFile = File.Create(codePath))
                {
                    BinaryWriter fullWriter = new BinaryWriter(fullFile);
                    BinaryWriter codeWriter = new BinaryWriter(codeFile);

                    fullWriter.Write(codeReader.ReadBytes(headerSize));

                    byte[] temp = codeReader.ReadBytes(code.Length - headerSize);

                    fullWriter.Write(temp);
                    codeWriter.Write(temp);

                    // Align to meet nvdisasm requirements.
                    while (codeFile.Length % 0x20 != 0)
                    {
                        codeWriter.Write(0);
                    }
                }
            }
        }

        private string FullDir()
        {
            return CreateAndReturn(Path.Combine(DumpDir(), "Full"));
        }

        private string CodeDir()
        {
            return CreateAndReturn(Path.Combine(DumpDir(), "Code"));
        }

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

        private static string CreateAndReturn(string dir)
        {
            Directory.CreateDirectory(dir);

            return dir;
        }
    }
}