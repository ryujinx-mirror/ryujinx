using Gtk;
using System.Reflection;

namespace Ryujinx.Ui.Diagnostic
{
    internal class UserErrorDialog : MessageDialog
    {
        private static string SetupGuideUrl = "https://github.com/Ryujinx/Ryujinx/wiki/Ryujinx-Setup-&-Configuration-Guide";
        private const int OkResponseId = 0;
        private const int SetupGuideResponseId = 1;

        private UserError _userError;

        private UserErrorDialog(UserError error) : base(null, DialogFlags.Modal, MessageType.Error, ButtonsType.None, null)
        {
            _userError = error;
            Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png");
            WindowPosition = WindowPosition.Center;
            Response += UserErrorDialog_Response;

            SetSizeRequest(120, 50);

            AddButton("OK", OkResponseId);

            bool isInSetupGuide = IsCoveredBySetupGuide(error);

            if (isInSetupGuide)
            {
                AddButton("Open the Setup Guide", SetupGuideResponseId);
            }

            string errorCode = GetErrorCode(error);

            SecondaryUseMarkup = true;

            Title = $"Ryujinx error ({errorCode})";
            Text = $"{errorCode}: {GetErrorTitle(error)}";
            SecondaryText = GetErrorDescription(error);

            if (isInSetupGuide)
            {
                SecondaryText += "\n<b>For more information on how to fix this error, follow our Setup Guide.</b>";
            }
        }

        private static string GetErrorCode(UserError error)
        {
            return $"RYU-{(uint)error:X4}";
        }

        private static string GetErrorTitle(UserError error)
        {
            switch (error)
            {
                case UserError.NoKeys:
                    return "Keys not found";
                case UserError.NoFirmware:
                    return "Firmware not found";
                case UserError.FirmwareParsingFailed:
                    return "Firmware parsing error";
                case UserError.ApplicationNotFound:
                    return "Application not found";
                case UserError.Unknown:
                    return "Unknown error";
                default:
                    return "Undefined error";
            }
        }

        private static string GetErrorDescription(UserError error)
        {
            switch (error)
            {
                case UserError.NoKeys:
                    return "Ryujinx was unable to find your 'prod.keys' file";
                case UserError.NoFirmware:
                    return "Ryujinx was unable to find any firmwares installed";
                case UserError.FirmwareParsingFailed:
                    return "Ryujinx was unable to parse the provided firmware. This is usually caused by outdated keys.";
                case UserError.ApplicationNotFound:
                    return "Ryujinx couldn't find a valid application at the given path.";
                case UserError.Unknown:
                    return "An unknown error occured!";
                default:
                    return "An undefined error occured! This shouldn't happen, please contact a dev!";
            }
        }

        private static bool IsCoveredBySetupGuide(UserError error)
        {
            switch (error)
            {
                case UserError.NoKeys:
                case UserError.NoFirmware:
                case UserError.FirmwareParsingFailed:
                    return true;
                default:
                    return false;
            }
        }

        private static string GetSetupGuideUrl(UserError error)
        {
            if (!IsCoveredBySetupGuide(error))
            {
                return null;
            }

            switch (error)
            {
                case UserError.NoKeys:
                    return SetupGuideUrl + "#initial-setup---placement-of-prodkeys";
                case UserError.NoFirmware:
                    return SetupGuideUrl + "#initial-setup-continued---installation-of-firmware";
            }

            return SetupGuideUrl;
        }

        private void UserErrorDialog_Response(object sender, ResponseArgs args)
        {
            int responseId = (int)args.ResponseId;

            if (responseId == SetupGuideResponseId)
            {
                UrlHelper.OpenUrl(GetSetupGuideUrl(_userError));
            }

            Dispose();
        }

        public static void CreateUserErrorDialog(UserError error)
        {
            new UserErrorDialog(error).Run();
        }
    }
}
