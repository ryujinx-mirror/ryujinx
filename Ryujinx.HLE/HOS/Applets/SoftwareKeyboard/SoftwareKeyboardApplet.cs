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
        private const string DEFAULT_NUMB = "1";
        private const string DEFAULT_TEXT = "Ryujinx";

        private const int STANDARD_BUFFER_SIZE    = 0x7D8;
        private const int INTERACTIVE_BUFFER_SIZE = 0x7D4;

        private SoftwareKeyboardState _state = SoftwareKeyboardState.Uninitialized;

        private AppletSession _normalSession;
        private AppletSession _interactiveSession;

        private SoftwareKeyboardConfig _keyboardConfig;

        private string _textValue = DEFAULT_TEXT;

        public event EventHandler AppletStateChanged;

        public SoftwareKeyboardApplet(Horizon system) { }

        public ResultCode Start(AppletSession normalSession,
                                AppletSession interactiveSession)
        {
            _normalSession      = normalSession;
            _interactiveSession = interactiveSession;

            _interactiveSession.DataAvailable += OnInteractiveData;

            var launchParams   = _normalSession.Pop();
            var keyboardConfig = _normalSession.Pop();
            var transferMemory = _normalSession.Pop();

            _keyboardConfig = ReadStruct<SoftwareKeyboardConfig>(keyboardConfig);

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
            // If the keyboard type is numbers only, we swap to a default
            // text that only contains numbers.
            if (_keyboardConfig.Type == SoftwareKeyboardType.NumbersOnly)
            {
                _textValue = DEFAULT_NUMB;
            }

            // If the max string length is 0, we set it to a large default
            // length.
            if (_keyboardConfig.StringLengthMax == 0)
            {
                _keyboardConfig.StringLengthMax = 100;
            }

            // If our default text is longer than the allowed length,
            // we truncate it.
            if (_textValue.Length > _keyboardConfig.StringLengthMax)
            {
                _textValue = _textValue.Substring(0, (int)_keyboardConfig.StringLengthMax);
            }

            if (!_keyboardConfig.CheckText)
            {
                // If the application doesn't need to validate the response,
                // we push the data to the non-interactive output buffer
                // and poll it for completion.             
                _state = SoftwareKeyboardState.Complete;

                _normalSession.Push(BuildResponse(_textValue, false));

                AppletStateChanged?.Invoke(this, null);
            }
            else
            {
                // The application needs to validate the response, so we
                // submit it to the interactive output buffer, and poll it
                // for validation. Once validated, the application will submit
                // back a validation status, which is handled in OnInteractiveDataPushIn.
                _state = SoftwareKeyboardState.ValidationPending;

                _interactiveSession.Push(BuildResponse(_textValue, true));
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
            int bufferSize = !interactive ? STANDARD_BUFFER_SIZE : INTERACTIVE_BUFFER_SIZE;

            using (MemoryStream stream = new MemoryStream(new byte[bufferSize]))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                byte[] output = Encoding.Unicode.GetBytes(text);

                if (!interactive)
                {
                    // Result Code
                    writer.Write((uint)0);
                }
                else
                {
                    // In interactive mode, we write the length of the text
                    // as a long, rather than a result code.
                    writer.Write((long)output.Length);
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
