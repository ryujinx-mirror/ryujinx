namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public abstract class ParentNode : BaseNode
    {
        public BaseNode Child { get; private set; }

        public ParentNode(NodeType Type, BaseNode Child) : base(Type)
        {
            this.Child = Child;
        }

        public override string GetName()
        {
            return Child.GetName();
        }
    }
}