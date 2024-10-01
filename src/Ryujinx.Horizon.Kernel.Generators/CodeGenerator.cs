using System.Text;

namespace Ryujinx.Horizon.Kernel.Generators
{
    class CodeGenerator
    {
        private const string Indent = "    ";
        private readonly StringBuilder _sb;
        private string _currentIndent;

        public CodeGenerator()
        {
            _sb = new StringBuilder();
        }

        public void EnterScope(string header = null)
        {
            if (header != null)
            {
                AppendLine(header);
            }

            AppendLine("{");
            IncreaseIndentation();
        }

        public void LeaveScope()
        {
            DecreaseIndentation();
            AppendLine("}");
        }

        public void IncreaseIndentation()
        {
            _currentIndent += Indent;
        }

        public void DecreaseIndentation()
        {
            _currentIndent = _currentIndent.Substring(0, _currentIndent.Length - Indent.Length);
        }

        public void AppendLine()
        {
            _sb.AppendLine();
        }

        public void AppendLine(string text)
        {
            _sb.AppendLine(_currentIndent + text);
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
