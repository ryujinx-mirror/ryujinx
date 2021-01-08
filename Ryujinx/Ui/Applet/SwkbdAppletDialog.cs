using Gtk;
using System;

namespace Ryujinx.Ui.Applet
{
    public class SwkbdAppletDialog : MessageDialog
    {
        private int _inputMin;
        private int _inputMax;

        private Predicate<int> _checkLength;

        private readonly Label _validationInfo;

        public Entry  InputEntry   { get; }
        public Button OkButton     { get; }
        public Button CancelButton { get; }

        public SwkbdAppletDialog(Window parent) : base(parent, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Question, ButtonsType.None, null)
        {
            SetDefaultSize(300, 0);

            _validationInfo = new Label()
            {
                Visible = false
            };

            InputEntry = new Entry()
            {
                Visible = true
            };

            InputEntry.Activated += OnInputActivated;
            InputEntry.Changed   += OnInputChanged;

            OkButton     = (Button)AddButton("OK",     ResponseType.Ok);
            CancelButton = (Button)AddButton("Cancel", ResponseType.Cancel);

            ((Box)MessageArea).PackEnd(_validationInfo, true, true, 0);
            ((Box)MessageArea).PackEnd(InputEntry,      true, true, 4);

            SetInputLengthValidation(0, int.MaxValue); // Disable by default.
        }

        public void SetInputLengthValidation(int min, int max)
        {
            _inputMin = Math.Min(min, max);
            _inputMax = Math.Max(min, max);

            _validationInfo.Visible = false;

            if (_inputMin <= 0 && _inputMax == int.MaxValue) // Disable.
            {
                _validationInfo.Visible = false;

                _checkLength = (length) => true;
            }
            else if (_inputMin > 0 && _inputMax == int.MaxValue)
            {
                _validationInfo.Visible = true;
                _validationInfo.Markup  = $"<i>Must be at least {_inputMin} characters long</i>";

                _checkLength = (length) => _inputMin <= length;
            }
            else
            {
                _validationInfo.Visible = true;
                _validationInfo.Markup  = $"<i>Must be {_inputMin}-{_inputMax} characters long</i>";

                _checkLength = (length) => _inputMin <= length && length <= _inputMax;
            }

            OnInputChanged(this, EventArgs.Empty);
        }

        private void OnInputActivated(object sender, EventArgs e)
        {
            if (OkButton.IsSensitive)
            {
                Respond(ResponseType.Ok);
            }
        }

        private void OnInputChanged(object sender, EventArgs e)
        {
            OkButton.Sensitive = _checkLength(InputEntry.Text.Length);
        }
    }
}