using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation
{
    class DelegateInfo
    {
        private readonly Delegate _dlg; // Ensure that this delegate will not be garbage collected.

        public IntPtr FuncPtr { get; }

        public DelegateInfo(Delegate dlg)
        {
            _dlg = dlg;

            FuncPtr = Marshal.GetFunctionPointerForDelegate<Delegate>(dlg);
        }
    }
}
