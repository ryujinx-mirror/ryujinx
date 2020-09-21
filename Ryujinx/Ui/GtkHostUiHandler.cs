using Gtk;
using Ryujinx.Common.Logging;
using Ryujinx.HLE;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService.ApplicationProxy.Types;
using System;
using System.Threading;

namespace Ryujinx.Ui
{
    internal class GtkHostUiHandler : IHostUiHandler
    {
        private readonly Window _parent;

        public GtkHostUiHandler(Window parent)
        {
            _parent = parent;
        }

        public bool DisplayMessageDialog(ControllerAppletUiArgs args)
        {
            string playerCount = args.PlayerCountMin == args.PlayerCountMax
                ? $"exactly {args.PlayerCountMin}"
                : $"{args.PlayerCountMin}-{args.PlayerCountMax}";

            string message =
                $"Application requests <b>{playerCount}</b> player(s) with:\n\n"
                + $"<tt><b>TYPES:</b> {args.SupportedStyles}</tt>\n\n"
                + $"<tt><b>PLAYERS:</b> {string.Join(", ", args.SupportedPlayers)}</tt>\n\n"
                + (args.IsDocked ? "Docked mode set. <tt>Handheld</tt> is also invalid.\n\n" : "")
                + "<i>Please reconfigure Input now and then press OK.</i>";

            return DisplayMessageDialog("Controller Applet", message);
        }

        public bool DisplayMessageDialog(string title, string message)
        {
            ManualResetEvent dialogCloseEvent = new ManualResetEvent(false);
            bool okPressed = false;

            Application.Invoke(delegate
            {
                MessageDialog msgDialog = null;
                try
                {
                    msgDialog = new MessageDialog(_parent, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, null)
                    {
                        Title = title,
                        Text = message,
                        UseMarkup = true
                    };

                    msgDialog.SetDefaultSize(400, 0);

                    msgDialog.Response += (object o, ResponseArgs args) =>
                    {
                        if (args.ResponseId == ResponseType.Ok) okPressed = true;
                        dialogCloseEvent.Set();
                        msgDialog?.Dispose();
                    };

                    msgDialog.Show();
                }
                catch (Exception e)
                {
                    Logger.Error?.Print(LogClass.Application, $"Error displaying Message Dialog: {e}");
                    dialogCloseEvent.Set();
                }
            });

            dialogCloseEvent.WaitOne();

            return okPressed;
        }

        public bool DisplayInputDialog(SoftwareKeyboardUiArgs args, out string userText)
        {
            ManualResetEvent dialogCloseEvent = new ManualResetEvent(false);
            bool okPressed = false;
            bool error = false;
            string inputText = args.InitialText ?? "";

            Application.Invoke(delegate
            {
                try
                {
                    var swkbdDialog = new InputDialog(_parent)
                    {
                        Title = "Software Keyboard",
                        Text = args.HeaderText,
                        SecondaryText = args.SubtitleText
                    };

                    swkbdDialog.InputEntry.Text = inputText;
                    swkbdDialog.InputEntry.PlaceholderText = args.GuideText;
                    swkbdDialog.OkButton.Label = args.SubmitText;

                    swkbdDialog.SetInputLengthValidation(args.StringLengthMin, args.StringLengthMax);

                    if (swkbdDialog.Run() == (int)ResponseType.Ok)
                    {
                        inputText = swkbdDialog.InputEntry.Text;
                        okPressed = true;
                    }

                    swkbdDialog.Dispose();
                }
                catch (Exception e)
                {
                    error = true;
                    Logger.Error?.Print(LogClass.Application, $"Error displaying Software Keyboard: {e}");
                }
                finally
                {
                    dialogCloseEvent.Set();
                }
            });

            dialogCloseEvent.WaitOne();

            userText = error ? null : inputText;

            return error || okPressed;
        }

        public void ExecuteProgram(HLE.Switch device, ProgramSpecifyKind kind, ulong value)
        {
            device.UserChannelPersistence.ExecuteProgram(kind, value);
            MainWindow.GlWidget?.Exit();
        }
    }
}
