using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class SpecialSubstitution : BaseNode
    {
        public enum SpecialType
        {
            Allocator,
            BasicString,
            String,
            IStream,
            OStream,
            IOStream,
        }

        private SpecialType SpecialSubstitutionKey;

        public SpecialSubstitution(SpecialType SpecialSubstitutionKey) : base(NodeType.SpecialSubstitution)
        {
            this.SpecialSubstitutionKey = SpecialSubstitutionKey;
        }

        public void SetExtended()
        {
            Type = NodeType.ExpandedSpecialSubstitution;
        }

        public override string GetName()
        {
            switch (SpecialSubstitutionKey)
            {
                case SpecialType.Allocator:
                    return "allocator";
                case SpecialType.BasicString:
                    return "basic_string";
                case SpecialType.String:
                    if (Type == NodeType.ExpandedSpecialSubstitution)
                    {
                        return "basic_string";
                    }

                    return "string";
                case SpecialType.IStream:
                    return "istream";
                case SpecialType.OStream:
                    return "ostream";
                case SpecialType.IOStream:
                    return "iostream";
            }

            return null;
        }

        private string GetExtendedName()
        {
            switch (SpecialSubstitutionKey)
            {
                case SpecialType.Allocator:
                    return "std::allocator";
                case SpecialType.BasicString:
                    return "std::basic_string";
                case SpecialType.String:
                    return "std::basic_string<char, std::char_traits<char>, std::allocator<char> >";
                case SpecialType.IStream:
                    return "std::basic_istream<char, std::char_traits<char> >";
                case SpecialType.OStream:
                    return "std::basic_ostream<char, std::char_traits<char> >";
                case SpecialType.IOStream:
                    return "std::basic_iostream<char, std::char_traits<char> >";
            }

            return null;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (Type == NodeType.ExpandedSpecialSubstitution)
            {
                Writer.Write(GetExtendedName());
            }
            else
            {
                Writer.Write("std::");
                Writer.Write(GetName());
            }
        }
    }
}