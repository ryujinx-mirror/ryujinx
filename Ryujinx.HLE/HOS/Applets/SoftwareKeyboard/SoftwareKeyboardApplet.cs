using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Applets.SoftwareKeyboard;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.HLE.HOS.Applets
{
    internal class SoftwareKeyboardApplet : IApplet
    {
        private const string DefaultText = "Ryujinx";

        private const long DebounceTimeMillis = 200;
        private const int ResetDelayMillis = 500;

        private readonly Switch _device;

        private const int StandardBufferSize    = 0x7D8;
        private const int InteractiveBufferSize = 0x7D4;
        private const int MaxUserWords          = 0x1388;

        private SoftwareKeyboardState _foregroundState = SoftwareKeyboardState.Uninitialized;
        private volatile InlineKeyboardState _backgroundState = InlineKeyboardState.Uninitialized;

        private bool _isBackground = false;
        private bool _alreadyShown = false;
        private volatile bool _useChangedStringV2 = false;

        private AppletSession _normalSession;
        private AppletSession _interactiveSession;

        // Configuration for foreground mode.
        private SoftwareKeyboardConfig _keyboardForegroundConfig;

        // Configuration for background (inline) mode.
        private SoftwareKeyboardInitialize   _keyboardBackgroundInitialize;
        private SoftwareKeyboardCalc         _keyboardBackgroundCalc;
        private SoftwareKeyboardCustomizeDic _keyboardBackgroundDic;
        private SoftwareKeyboardDictSet      _keyboardBackgroundDictSet;
        private SoftwareKeyboardUserWord[]   _keyboardBackgroundUserWords;

        private byte[] _transferMemory;

        private string   _textValue = "";
        private bool     _okPressed = false;
        private Encoding _encoding  = Encoding.Unicode;
        private long     _lastTextSetMillis = 0;
        private bool     _lastWasHidden = false;

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

            _alreadyShown = false;
            _useChangedStringV2 = false;

            var launchParams   = _normalSession.Pop();
            var keyboardConfig = _normalSession.Pop();

            if (keyboardConfig.Length == Marshal.SizeOf<SoftwareKeyboardInitialize>())
            {
                // Initialize the keyboard applet in background mode.

                _isBackground = true;

                _keyboardBackgroundInitialize = ReadStruct<SoftwareKeyboardInitialize>(keyboardConfig);
                _backgroundState = InlineKeyboardState.Uninitialized;

                return ResultCode.Success;
            }
            else
            {
                // Initialize the keyboard applet in foreground mode.

                _isBackground = false;

                if (keyboardConfig.Length < Marshal.SizeOf<SoftwareKeyboardConfig>())
                {
                    Logger.Error?.Print(LogClass.ServiceAm, $"SoftwareKeyboardConfig size mismatch. Expected {Marshal.SizeOf<SoftwareKeyboardConfig>():x}. Got {keyboardConfig.Length:x}");
                }
                else
                {
                    _keyboardForegroundConfig = ReadStruct<SoftwareKeyboardConfig>(keyboardConfig);
                }

                if (!_normalSession.TryPop(out _transferMemory))
                {
                    Logger.Error?.Print(LogClass.ServiceAm, "SwKbd Transfer Memory is null");
                }

                if (_keyboardForegroundConfig.UseUtf8)
                {
                    _encoding = Encoding.UTF8;
                }

                _foregroundState = SoftwareKeyboardState.Ready;

                ExecuteForegroundKeyboard();

                return ResultCode.Success;
            }
        }

        public ResultCode GetResult()
        {
            return ResultCode.Success;
        }

        private InlineKeyboardState GetInlineState()
        {
            return _backgroundState;
        }

        private void SetInlineState(InlineKeyboardState state)
        {
            _backgroundState = state;
        }

        private void ExecuteForegroundKeyboard()
        {
            string initialText = null;

            // Initial Text is always encoded as a UTF-16 string in the work buffer (passed as transfer memory)
            // InitialStringOffset points to the memory offset and InitialStringLength is the number of UTF-16 characters
            if (_transferMemory != null && _keyboardForegroundConfig.InitialStringLength > 0)
            {
                initialText = Encoding.Unicode.GetString(_transferMemory, _keyboardForegroundConfig.InitialStringOffset,
                    2 * _keyboardForegroundConfig.InitialStringLength);
            }

            // If the max string length is 0, we set it to a large default
            // length.
            if (_keyboardForegroundConfig.StringLengthMax == 0)
            {
                _keyboardForegroundConfig.StringLengthMax = 100;
            }

            var args = new SoftwareKeyboardUiArgs
            {
                HeaderText = _keyboardForegroundConfig.HeaderText,
                SubtitleText = _keyboardForegroundConfig.SubtitleText,
                GuideText = _keyboardForegroundConfig.GuideText,
                SubmitText = (!string.IsNullOrWhiteSpace(_keyboardForegroundConfig.SubmitText) ?
                    _keyboardForegroundConfig.SubmitText : "OK"),
                StringLengthMin = _keyboardForegroundConfig.StringLengthMin,
                StringLengthMax = _keyboardForegroundConfig.StringLengthMax,
                InitialText = initialText
            };

            // Call the configured GUI handler to get user's input
            if (_device.UiHandler == null)
            {
                Logger.Warning?.Print(LogClass.Application, "GUI Handler is not set. Falling back to default");
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
            while (_textValue.Length < _keyboardForegroundConfig.StringLengthMin)
            {
                _textValue = String.Join(" ", _textValue, _textValue);
            }

            // If our default text is longer than the allowed length,
            // we truncate it.
            if (_textValue.Length > _keyboardForegroundConfig.StringLengthMax)
            {
                _textValue = _textValue.Substring(0, (int)_keyboardForegroundConfig.StringLengthMax);
            }

            // Does the application want to validate the text itself?
            if (_keyboardForegroundConfig.CheckText)
            {
                // The application needs to validate the response, so we
                // submit it to the interactive output buffer, and poll it
                // for validation. Once validated, the application will submit
                // back a validation status, which is handled in OnInteractiveDataPushIn.
                _foregroundState = SoftwareKeyboardState.ValidationPending;

                _interactiveSession.Push(BuildResponse(_textValue, true));
            }
            else
            {
                // If the application doesn't need to validate the response,
                // we push the data to the non-interactive output buffer
                // and poll it for completion.
                _foregroundState = SoftwareKeyboardState.Complete;

                _normalSession.Push(BuildResponse(_textValue, false));

                AppletStateChanged?.Invoke(this, null);
            }
        }

        private void OnInteractiveData(object sender, EventArgs e)
        {
            // Obtain the validation status response.
            var data = _interactiveSession.Pop();

            if (_isBackground)
            {
                OnBackgroundInteractiveData(data);
            }
            else
            {
                OnForegroundInteractiveData(data);
            }
        }

        private void OnForegroundInteractiveData(byte[] data)
        {
            if (_foregroundState == SoftwareKeyboardState.ValidationPending)
            {
                // TODO(jduncantor):
                // If application rejects our "attempt", submit another attempt,
                // and put the applet back in PendingValidation state.

                // For now we assume success, so we push the final result
                // to the standard output buffer and carry on our merry way.
                _normalSession.Push(BuildResponse(_textValue, false));

                AppletStateChanged?.Invoke(this, null);

                _foregroundState = SoftwareKeyboardState.Complete;
            }
            else if(_foregroundState == SoftwareKeyboardState.Complete)
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

        private void OnBackgroundInteractiveData(byte[] data)
        {
            // WARNING: Only invoke applet state changes after an explicit finalization
            // request from the game, this is because the inline keyboard is expected to
            // keep running in the background sending data by itself.

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                InlineKeyboardRequest request = (InlineKeyboardRequest)reader.ReadUInt32();
                InlineKeyboardState state = GetInlineState();
                long remaining;

                Logger.Debug?.Print(LogClass.ServiceAm, $"Keyboard received command {request} in state {state}");

                switch (request)
                {
                    case InlineKeyboardRequest.UseChangedStringV2:
                        _useChangedStringV2 = true;
                        break;
                    case InlineKeyboardRequest.UseMovedCursorV2:
                        // Not used because we only reply with the final string.
                        break;
                    case InlineKeyboardRequest.SetUserWordInfo:
                        // Read the user word info data.
                        remaining = stream.Length - stream.Position;
                        if (remaining < sizeof(int))
                        {
                            Logger.Warning?.Print(LogClass.ServiceAm, $"Received invalid Software Keyboard User Word Info of {remaining} bytes");
                        }
                        else
                        {
                            int wordsCount = reader.ReadInt32();
                            int wordSize = Marshal.SizeOf<SoftwareKeyboardUserWord>();
                            remaining = stream.Length - stream.Position;

                            if (wordsCount > MaxUserWords)
                            {
                                Logger.Warning?.Print(LogClass.ServiceAm, $"Received {wordsCount} User Words but the maximum is {MaxUserWords}");
                            }
                            else if (wordsCount * wordSize != remaining)
                            {
                                Logger.Warning?.Print(LogClass.ServiceAm, $"Received invalid Software Keyboard User Word Info data of {remaining} bytes for {wordsCount} words");
                            }
                            else
                            {
                                _keyboardBackgroundUserWords = new SoftwareKeyboardUserWord[wordsCount];

                                for (int word = 0; word < wordsCount; word++)
                                {
                                    byte[] wordData = reader.ReadBytes(wordSize);
                                    _keyboardBackgroundUserWords[word] = ReadStruct<SoftwareKeyboardUserWord>(wordData);
                                }
                            }
                        }
                        _interactiveSession.Push(InlineResponses.ReleasedUserWordInfo(state));
                        break;
                    case InlineKeyboardRequest.SetCustomizeDic:
                        // Read the custom dic data.
                        remaining = stream.Length - stream.Position;
                        if (remaining != Marshal.SizeOf<SoftwareKeyboardCustomizeDic>())
                        {
                            Logger.Warning?.Print(LogClass.ServiceAm, $"Received invalid Software Keyboard Customize Dic of {remaining} bytes");
                        }
                        else
                        {
                            var keyboardDicData = reader.ReadBytes((int)remaining);
                            _keyboardBackgroundDic = ReadStruct<SoftwareKeyboardCustomizeDic>(keyboardDicData);
                        }
                        _interactiveSession.Push(InlineResponses.UnsetCustomizeDic(state));
                        break;
                    case InlineKeyboardRequest.SetCustomizedDictionaries:
                        // Read the custom dictionaries data.
                        remaining = stream.Length - stream.Position;
                        if (remaining != Marshal.SizeOf<SoftwareKeyboardDictSet>())
                        {
                            Logger.Warning?.Print(LogClass.ServiceAm, $"Received invalid Software Keyboard DictSet of {remaining} bytes");
                        }
                        else
                        {
                            var keyboardDictData = reader.ReadBytes((int)remaining);
                            _keyboardBackgroundDictSet = ReadStruct<SoftwareKeyboardDictSet>(keyboardDictData);
                        }
                        _interactiveSession.Push(InlineResponses.UnsetCustomizedDictionaries(state));
                        break;
                    case InlineKeyboardRequest.Calc:
                        // The Calc request tells the Applet to enter the main input handling loop, which will end
                        // with either a text being submitted or a cancel request from the user.

                        // NOTE: Some Calc requests happen early in the application and are not meant to be shown. This possibly
                        // happens because the game has complete control over when the inline keyboard is drawn, but here it
                        // would cause a dialog to pop in the emulator, which is inconvenient. An algorithm is applied to
                        // decide whether it is a dummy Calc or not, but regardless of the result, the dummy Calc appears to
                        // never happen twice, so the keyboard will always show if it has already been shown before.
                        bool shouldShowKeyboard = _alreadyShown;
                        _alreadyShown = true;

                        // Read the Calc data.
                        remaining = stream.Length - stream.Position;
                        if (remaining != Marshal.SizeOf<SoftwareKeyboardCalc>())
                        {
                            Logger.Error?.Print(LogClass.ServiceAm, $"Received invalid Software Keyboard Calc of {remaining} bytes");
                        }
                        else
                        {
                            var keyboardCalcData = reader.ReadBytes((int)remaining);
                            _keyboardBackgroundCalc = ReadStruct<SoftwareKeyboardCalc>(keyboardCalcData);

                            // Check if the application expects UTF8 encoding instead of UTF16.
                            if (_keyboardBackgroundCalc.UseUtf8)
                            {
                                _encoding = Encoding.UTF8;
                            }

                            // Force showing the keyboard regardless of the state, an unwanted
                            // input dialog may show, but it is better than a soft lock.
                            if (_keyboardBackgroundCalc.Appear.ShouldBeHidden == 0)
                            {
                                shouldShowKeyboard = true;
                            }
                        }
                        // Send an initialization finished signal.
                        state = InlineKeyboardState.Ready;
                        SetInlineState(state);
                        _interactiveSession.Push(InlineResponses.FinishedInitialize(state));
                        // Start a task with the GUI handler to get user's input.
                        new Task(() => { GetInputTextAndSend(shouldShowKeyboard, state); }).Start();
                        break;
                    case InlineKeyboardRequest.Finalize:
                        // The calling application wants to close the keyboard applet and will wait for a state change.
                        _backgroundState = InlineKeyboardState.Uninitialized;
                        AppletStateChanged?.Invoke(this, null);
                        break;
                    default:
                        // We shouldn't be able to get here through standard swkbd execution.
                        Logger.Warning?.Print(LogClass.ServiceAm, $"Invalid Software Keyboard request {request} during state {_backgroundState}");
                        _interactiveSession.Push(InlineResponses.Default(state));
                        break;
                }
            }
        }

        private void GetInputTextAndSend(bool shouldShowKeyboard, InlineKeyboardState oldState)
        {
            bool submit = true;

            // Use the text specified by the Calc if it is available, otherwise use the default one.
            string inputText = (!string.IsNullOrWhiteSpace(_keyboardBackgroundCalc.InputText) ?
                _keyboardBackgroundCalc.InputText : DefaultText);

            // Compute the elapsed time for the debouncing algorithm.
            long currentMillis = PerformanceCounter.ElapsedMilliseconds;
            long inputElapsedMillis = currentMillis - _lastTextSetMillis;

            // Reset the input text before submitting the final result, that's because some games do not expect
            // consecutive submissions to abruptly shrink and they will crash if it happens. Changing the string
            // before the final submission prevents that.
            InlineKeyboardState newState = InlineKeyboardState.DataAvailable;
            SetInlineState(newState);
            ChangedString("", newState);

            if (!_lastWasHidden && (inputElapsedMillis < DebounceTimeMillis))
            {
                // A repeated Calc request has been received without player interaction, after the input has been
                // sent. This behavior happens in some games, so instead of showing another dialog, just apply a
                // time-based debouncing algorithm and repeat the last submission, either a value or a cancel.
                // It is also possible that the first Calc request was hidden by accident, in this case use the
                // debouncing as an oportunity to properly ask for input.
                inputText = _textValue;
                submit = _textValue != null;
                _lastWasHidden = false;

                Logger.Warning?.Print(LogClass.Application, "Debouncing repeated keyboard request");
            }
            else if (!shouldShowKeyboard)
            {
                // Submit the default text to avoid soft locking if the keyboard was ignored by
                // accident. It's better to change the name than being locked out of the game.
                inputText = DefaultText;
                _lastWasHidden = true;

                Logger.Debug?.Print(LogClass.Application, "Received a dummy Calc, keyboard will not be shown");
            }
            else if (_device.UiHandler == null)
            {
                Logger.Warning?.Print(LogClass.Application, "GUI Handler is not set. Falling back to default");
                _lastWasHidden = false;
            }
            else
            {
                // Call the configured GUI handler to get user's input.
                var args = new SoftwareKeyboardUiArgs
                {
                    HeaderText = "", // The inline keyboard lacks these texts
                    SubtitleText = "",
                    GuideText = "",
                    SubmitText = (!string.IsNullOrWhiteSpace(_keyboardBackgroundCalc.Appear.OkText) ?
                        _keyboardBackgroundCalc.Appear.OkText : "OK"),
                    StringLengthMin = 0,
                    StringLengthMax = 100,
                    InitialText = inputText
                };

                submit = _device.UiHandler.DisplayInputDialog(args, out inputText);
                inputText = submit ? inputText : null;
                _lastWasHidden = false;
            }

            // The 'Complete' state indicates the Calc request has been fulfilled by the applet.
            newState = InlineKeyboardState.Complete;

            if (submit)
            {
                Logger.Debug?.Print(LogClass.ServiceAm, "Sending keyboard OK");
                DecidedEnter(inputText, newState);
            }
            else
            {
                Logger.Debug?.Print(LogClass.ServiceAm, "Sending keyboard Cancel");
                DecidedCancel(newState);
            }

            _interactiveSession.Push(InlineResponses.Default(newState));

            // The constant calls to PopInteractiveData suggest that the keyboard applet continuously reports
            // data back to the application and this can also be time-sensitive. Pushing a state reset right
            // after the data has been sent does not work properly and the application will soft-lock. This
            // delay gives time for the application to catch up with the data and properly process the state
            // reset.
            Thread.Sleep(ResetDelayMillis);

            // 'Initialized' is the only known state so far that does not soft-lock the keyboard after use.
            newState = InlineKeyboardState.Initialized;

            Logger.Debug?.Print(LogClass.ServiceAm, $"Resetting state of the keyboard to {newState}");

            SetInlineState(newState);
            _interactiveSession.Push(InlineResponses.Default(newState));

            // Keep the text and the timestamp of the input for the debouncing algorithm.
            _textValue = inputText;
            _lastTextSetMillis = PerformanceCounter.ElapsedMilliseconds;
        }

        private void ChangedString(string text, InlineKeyboardState state)
        {
            if (_encoding == Encoding.UTF8)
            {
                if (_useChangedStringV2)
                {
                    _interactiveSession.Push(InlineResponses.ChangedStringUtf8V2(text, state));
                }
                else
                {
                    _interactiveSession.Push(InlineResponses.ChangedStringUtf8(text, state));
                }
            }
            else
            {
                if (_useChangedStringV2)
                {
                    _interactiveSession.Push(InlineResponses.ChangedStringV2(text, state));
                }
                else
                {
                    _interactiveSession.Push(InlineResponses.ChangedString(text, state));
                }
            }
        }

        private void DecidedEnter(string text, InlineKeyboardState state)
        {
            if (_encoding == Encoding.UTF8)
            {
                _interactiveSession.Push(InlineResponses.DecidedEnterUtf8(text, state));
            }
            else
            {
                _interactiveSession.Push(InlineResponses.DecidedEnter(text, state));
            }
        }

        private void DecidedCancel(InlineKeyboardState state)
        {
            _interactiveSession.Push(InlineResponses.DecidedCancel(state));
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
