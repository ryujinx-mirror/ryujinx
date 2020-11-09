using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    class CodeGenContext
    {
        public const string Tab = "    ";

        private readonly StructuredProgramInfo _info;

        public StructuredFunction CurrentFunction { get; set; }

        public ShaderConfig Config { get; }

        public bool CbIndexable => _info.UsesCbIndexing;

        public List<BufferDescriptor>  CBufferDescriptors { get; }
        public List<BufferDescriptor>  SBufferDescriptors { get; }
        public List<TextureDescriptor> TextureDescriptors { get; }
        public List<TextureDescriptor> ImageDescriptors   { get; }

        public OperandManager OperandManager { get; }

        private StringBuilder _sb;

        private int _level;

        private string _indentation;

        public CodeGenContext(StructuredProgramInfo info, ShaderConfig config)
        {
            _info = info;
            Config = config;

            CBufferDescriptors = new List<BufferDescriptor>();
            SBufferDescriptors = new List<BufferDescriptor>();
            TextureDescriptors = new List<TextureDescriptor>();
            ImageDescriptors   = new List<TextureDescriptor>();

            OperandManager = new OperandManager();

            _sb = new StringBuilder();
        }

        public void AppendLine()
        {
            _sb.AppendLine();
        }

        public void AppendLine(string str)
        {
            _sb.AppendLine(_indentation + str);
        }

        public string GetCode()
        {
            return _sb.ToString();
        }

        public void EnterScope()
        {
            AppendLine("{");

            _level++;

            UpdateIndentation();
        }

        public void LeaveScope(string suffix = "")
        {
            if (_level == 0)
            {
                return;
            }

            _level--;

            UpdateIndentation();

            AppendLine("}" + suffix);
        }

        private int FindDescriptorIndex(List<TextureDescriptor> list, AstTextureOperation texOp)
        {
            return list.FindIndex(descriptor =>
                descriptor.Type == texOp.Type &&
                descriptor.CbufSlot == texOp.CbufSlot &&
                descriptor.HandleIndex == texOp.Handle &&
                descriptor.Format == texOp.Format);
        }

        public int FindTextureDescriptorIndex(AstTextureOperation texOp)
        {
            return FindDescriptorIndex(TextureDescriptors, texOp);
        }

        public int FindImageDescriptorIndex(AstTextureOperation texOp)
        {
            return FindDescriptorIndex(ImageDescriptors, texOp);
        }

        public StructuredFunction GetFunction(int id)
        {
            return _info.Functions[id];
        }

        private void UpdateIndentation()
        {
            _indentation = GetIndentation(_level);
        }

        private static string GetIndentation(int level)
        {
            string indentation = string.Empty;

            for (int index = 0; index < level; index++)
            {
                indentation += Tab;
            }

            return indentation;
        }
    }
}