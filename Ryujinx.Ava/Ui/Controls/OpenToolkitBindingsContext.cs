using OpenTK;
using System;

namespace Ryujinx.Ava.Ui.Controls
{
    internal class OpenToolkitBindingsContext : IBindingsContext
    {
        private readonly Func<string, IntPtr> _getProcAddress;

        public OpenToolkitBindingsContext(Func<string, IntPtr> getProcAddress)
        {
            _getProcAddress = getProcAddress;
        }

        public IntPtr GetProcAddress(string procName)
        {
            return _getProcAddress(procName);
        }
    }
}