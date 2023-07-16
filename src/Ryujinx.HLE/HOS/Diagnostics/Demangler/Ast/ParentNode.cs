namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public abstract class ParentNode : BaseNode
    {
        public BaseNode Child { get; private set; }

        public ParentNode(NodeType type, BaseNode child) : base(type)
        {
            Child = child;
        }

        public override string GetName()
        {
            return Child.GetName();
        }
    }
}
