using System;
using System.IO;


namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class CastExpression : BaseNode
    {
        private string   Kind;
        private BaseNode To;
        private BaseNode From;

        public CastExpression(string Kind, BaseNode To, BaseNode From) : base(NodeType.CastExpression)
        {
            this.Kind = Kind;
            this.To   = To;
            this.From = From;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write(Kind);
            Writer.Write("<");
            To.PrintLeft(Writer);
            Writer.Write(">(");
            From.PrintLeft(Writer);
            Writer.Write(")");            
        }
    }
}