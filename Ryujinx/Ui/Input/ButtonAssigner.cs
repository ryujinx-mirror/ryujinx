using Ryujinx.Common.Configuration.Hid;

namespace Ryujinx.Ui.Input
{
    interface ButtonAssigner
    {
        void Init();

        void ReadInput();

        bool HasAnyButtonPressed();

        bool ShouldCancel();

        string GetPressedButton();
    }
}