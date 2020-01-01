using System;

namespace Ryujinx.Graphics.Shader.Translation
{
    public struct TranslatorCallbacks
    {
        internal Func<QueryInfoName, int, int> QueryInfo { get; }

        internal Action<string> PrintLog { get; }

        public TranslatorCallbacks(Func<QueryInfoName, int, int> queryInfoCallback, Action<string> printLogCallback)
        {
            QueryInfo = queryInfoCallback;
            PrintLog  = printLogCallback;
        }
    }
}
