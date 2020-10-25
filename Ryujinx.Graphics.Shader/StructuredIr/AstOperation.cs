using Ryujinx.Graphics.Shader.IntermediateRepresentation;

using static Ryujinx.Graphics.Shader.StructuredIr.AstHelper;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class AstOperation : AstNode
    {
        public Instruction Inst { get; }

        public int Index { get; }

        private IAstNode[] _sources;

        public int SourcesCount => _sources.Length;

        public AstOperation(Instruction inst, IAstNode[] sources, int sourcesCount)
        {
            Inst     = inst;
            _sources = sources;

            for (int index = 0; index < sources.Length; index++)
            {
                if (index < sourcesCount)
                {
                    AddUse(sources[index], this);
                }
                else
                {
                    AddDef(sources[index], this);
                }
            }

            Index = 0;
        }

        public AstOperation(Instruction inst, int index, IAstNode[] sources, int sourcesCount) : this(inst, sources, sourcesCount)
        {
            Index = index;
        }

        public AstOperation(Instruction inst, params IAstNode[] sources) : this(inst, sources, sources.Length)
        {
        }

        public IAstNode GetSource(int index)
        {
            return _sources[index];
        }

        public void SetSource(int index, IAstNode source)
        {
            RemoveUse(_sources[index], this);

            AddUse(source, this);

            _sources[index] = source;
        }
    }
}