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

        public AstOperation(Instruction inst, params IAstNode[] sources)
        {
            Inst     = inst;
            _sources = sources;

            foreach (IAstNode source in sources)
            {
                AddUse(source, this);
            }

            Index = 0;
        }

        public AstOperation(Instruction inst, int index, params IAstNode[] sources) : this(inst, sources)
        {
            Index = index;
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