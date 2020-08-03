using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Applets.SoftwareKeyboard;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.HLE.HOS.Applets
{
    internal class SoftwareKeyboardApplet : IApplet
    {
        private const string DefaultText = "Ryujinx";

        private readonly Switch _device;

        private const int StandardBufferSize    = 0x7D8;
        private const int InteractiveBufferSize = 0x7D4;

        private SoftwareKeyboardState _state = SoftwareKeyboardState.Uninitialized;

        private AppletSession _normalSession;
        private AppletSession _interactiveSession;

        private SoftwareKeyboardConfig _keyboardConfig;
        private byte[] _transferMemory;

        private string   _textValue = null;
        private bool     _okPressed = false;
        private Encoding _encoding  = Encoding.Unicode;

        public event EventHandler AppletStateChanged;

        public SoftwareKeyboardApplet(Horizon system)
        {
            _device = system.Device;
        }

        public ResultCode Start(AppletSession normalSession,
                                AppletSession interactiveSession)
        {
            _normalSession      = normalSession;
            _interactiveSession = interactiveSession;

            _interactiveSession.DataAvailable += OnInteractiveData;

            var launchParams   = _normalSession.Pop();
            var keyboardConfig = _normalSession.Pop();

            if (keyboardConfig.Length < Marshal.SizeOf<SoftwareKeyboardConfig>())
            {
                Logger.Error?.Print(LogClass.ServiceAm, $"SoftwareKeyboardConfig size mismatch. Expected {Marshal.SizeOf<SoftwareKeyboardConfig>():x}. Got {keyboardConfig.Length:x}");
            }
            else
            {
                _keyboardConfig = ReadStruct<SoftwareKeyboardConfig>(keyboardConfig);
            }

            if (!_normalSession.TryPop(out _transferMemory))
            {
                Logger.Error?.Print(LogClass.ServiceAm, "SwKbd Transfer Memory is null");
            }

            if (_keyboardConfig.UseUtf8)
            {
                _encoding = Encoding.UTF8;
            }

            _state = SoftwareKeyboardState.Ready;

            Execute();

            return ResultCode.Success;
        }

        public ResultCode GetResult()
        {
            return ResultCode.Success;
        }

        private void Execute()
        {
            string initialText = null;

            // Initial Text is always encoded as a UTF-16 string in the work buffer (passed as transfer memory)
            // InitialStringOffset points to the memory offset and InitialStringLength is the number of UTF-16 characters
            if (_transferMemory != null && _keyboardConfig.InitialStringLength > 0)
            {
                initialText = Encoding.Unicode.GetString(_transferMemory, _keyboardConfig.InitialStringOffset, 2 * _keyboardConfig.InitialStringLength);
            }

            // If the max string length is 0, we set it to a large default
            // length.
            if (_keyboardConfig.StringLengthMax == 0)
            {
                _keyboardConfig.StringLengthMax = 100;
            }

            var args = new SoftwareKeyboardUiArgs
            {
                HeaderText = _keyboardConfig.HeaderText,
                SubtitleText = _keyboardConfig.SubtitleText,
                GuideText = _keyboardConfig.GuideText,
                SubmitText = (!string.IsNullOrWhiteSpace(_keyboardConfig.SubmitText) ? _keyboardConfig.SubmitText : "OK"),
                StringLengthMin = _keyboardConfig.StringLengthMin, 
                StringLengthMax = _keyboardConfig.StringLengthMax,
                InitialText = initialText
            };

            // Call the configured GUI handler to get user's input
            if (_device.UiHandler == null)
            {
                Logger.Warning?.Print(LogClass.Application, $"GUI Handler is not set. Falling back to default");
                _okPressed = true;
            }
            else
            {
                _okPressed = _device.UiHandler.DisplayInputDialog(args, out _textValue);
            }

            _textValue ??= initialText ?? DefaultText;

            // If the game requests a string with a minimum length less
            // than our default text, repeat our default text until we meet
            // the minimum length requirement. 
            // This should always be done before the text truncation step.
            while (_textValue.Length < _keyboardConfig.StringLengthMin)
            {
                _textValue = String.Join(" ", _textValue, _textValue);
            }

            // If our default text is longer than the allowed length,
            // we truncate it.
            if (_textValue.Length > _keyboardConfig.StringLengthMax)
            {
                _textValue = _textValue.Substring(0, (int)_keyboardConfig.StringLengthMax);
            }

            // Does the application want to validate the text itself?
            if (_keyboardConfig.CheckText)
            {
                // The application needs to validate the response, so we
                // submit it to the interactive output buffer, and poll it
                // for validation. Once validated, the application will submit
                // back a validation status, which is handled in OnInteractiveDataPushIn.
                _state = SoftwareKeyboardState.ValidationPending;

                _interactiveSession.Push(BuildResponse(_textValue, true));
            }
            else
            {
                // If the application doesn't need to validate the response,
                // we push the data to the non-interactive output buffer
                // and poll it for completion.             
                _state = SoftwareKeyboardState.Complete;

                _normalSession.Push(BuildResponse(_textValue, false));

                AppletStateChanged?.Invoke(this, null);
            }
        }

        private void OnInteractiveData(object sender, EventArgs e)
        {
            // Obtain the validation status response, 
            var data = _interactiveSession.Pop();

            if (_state == SoftwareKeyboardState.ValidationPending)
            {
                // TODO(jduncantor):
                // If application rejects our "attempt", submit another attempt,
                // and put the applet back in PendingValidation state.

                // For now we assume success, so we push the final result 
                // to the standard output buffer and carry on our merry way.
                _normalSession.Push(BuildResponse(_textValue, false));

                AppletStateChanged?.Invoke(this, null);

                _state = SoftwareKeyboardState.Complete;
            }
            else if(_state == SoftwareKeyboardState.Complete)
            {
                // If we have already completed, we push the result text
                // back on the output buffer and poll the application.
                _normalSession.Push(BuildResponse(_textValue, false));

                AppletStateChanged?.Invoke(this, null);
            }
            else
            {
                // We shouldn't be able to get here through standard swkbd execution.
                throw new InvalidOperationException("Software Keyboard is in an invalid state.");
            }
        }

        private byte[] BuildResponse(string text, bool interactive)
        {
            int bufferSize = interactive ? InteractiveBufferSize : StandardBufferSize;

            using (MemoryStream stream = new MemoryStream(new byte[bufferSize]))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                byte[] output = _encoding.GetBytes(text);

                if (!interactive)
                {
                    // Result Code
                    writer.Write(_okPressed ? 0U : 1U);
                }
                else
                {
                    // In interactive mode, we write the length of the text as a long, rather than
                    // a result code. This field is inclusive of the 64-bit size.
                    writer.Write((long)output.Length + 8);
                }

                writer.Write(output);

                return stream.ToArray();
            }
        }

        private static T ReadStruct<T>(byte[] data)
            where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {    
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
