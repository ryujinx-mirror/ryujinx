using System.IO;

namespace Ryujinx.Graphics.Gpu.Engine
{
    class ShaderDumper
    {
        private const int ShaderHeaderSize = 0x50;

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

        public void Dump(ulong gpuVa, bool compute)
        {
            _dumpPath = GraphicsConfig.ShadersDumpPath;

            if (string.IsNullOrWhiteSpace(_dumpPath))
            {
                return;
            }

            string fileName = "Shader" + _dumpIndex.ToString("d4") + ".bin";

            string fullPath = Path.Combine(FullDir(), fileName);
            string codePath = Path.Combine(CodeDir(), fileName);

            _dumpIndex++;

            ulong headerSize = compute ? 0UL : ShaderHeaderSize;

            using (FileStream fullFile = File.Create(fullPath))
            using (FileStream codeFile = File.Create(codePath))
            {
                BinaryWriter fullWriter = new BinaryWriter(fullFile);
                BinaryWriter codeWriter = new BinaryWriter(codeFile);

                for (ulong i = 0; i < headerSize; i += 4)
                {
                    fullWriter.Write(_context.MemoryAccessor.ReadInt32(gpuVa + i));
                }

                ulong offset = 0;

                ulong instruction = 0;

                // Dump until a NOP instruction is found.
                while ((instruction >> 48 & 0xfff8) != 0x50b0)
                {
                    uint word0 = (uint)_context.MemoryAccessor.ReadInt32(gpuVa + headerSize + offset + 0);
                    uint word1 = (uint)_context.MemoryAccessor.ReadInt32(gpuVa + headerSize + offset + 4);

                    instruction = word0 | (ulong)word1 << 32;

                    // Zero instructions (other kind of NOP) stop immediately,
                    // this is to avoid two rows of zeroes.
                    if (instruction == 0)
                    {
                        break;
                    }

                    fullWriter.Write(instruction);
                    codeWriter.Write(instruction);

                    offset += 8;
                }

                // Align to meet nvdisasm requirements.
                while (offset % 0x20 != 0)
                {
                    fullWriter.Write(0);
                    codeWriter.Write(0);

                    offset += 4;
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