using System;

namespace Ryujinx.HLE.UI
{
    public interface IDynamicTextInputHandler : IDisposable
    {
        event DynamicTextChangedHandler TextChangedEvent;
        event KeyPressedHandler KeyPressedEvent;
        event KeyReleasedHandler KeyReleasedEvent;

        bool TextProcessingEnabled { get; set; }

        void SetText(string text, int cursorBegin);
        void SetText(string text, int cursorBegin, int cursorEnd);
    }
}
