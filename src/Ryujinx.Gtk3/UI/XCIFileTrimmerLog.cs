using Ryujinx.Common.Logging;
using System;

namespace Ryujinx.UI
{
    public class XCIFileTrimmerLog : Ryujinx.Common.Logging.XCIFileTrimmerLog
    {
        private readonly MainWindow _mainWindow;

        public XCIFileTrimmerLog(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public override void Progress(long current, long total, string text, bool complete)
        {
            if (!complete)
            {
                _mainWindow.UpdateProgress((double)current / (double)total);
            }
            else
            {
                _mainWindow.EndProgress();
            }
        }
    }
}
