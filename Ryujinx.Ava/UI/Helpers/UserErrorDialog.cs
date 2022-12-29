using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Windows;
using Ryujinx.Ui.Common;
using Ryujinx.Ui.Common.Helper;
using System.Threading.Tasks;

namespace Ryujinx.Ava.UI.Helpers
{
    internal class UserErrorDialog
    {
        private const string SetupGuideUrl = "https://github.com/Ryujinx/Ryujinx/wiki/Ryujinx-Setup-&-Configuration-Guide";

        private static string GetErrorCode(UserError error)
        {
            return $"RYU-{(uint)error:X4}";
        }

        private static string GetErrorTitle(UserError error)
        {
            return error switch
            {
                UserError.NoKeys => LocaleManager.Instance["UserErrorNoKeys"],
                UserError.NoFirmware => LocaleManager.Instance["UserErrorNoFirmware"],
                UserError.FirmwareParsingFailed => LocaleManager.Instance["UserErrorFirmwareParsingFailed"],
                UserError.ApplicationNotFound => LocaleManager.Instance["UserErrorApplicationNotFound"],
                UserError.Unknown => LocaleManager.Instance["UserErrorUnknown"],
                _ => LocaleManager.Instance["UserErrorUndefined"]
            };
        }

        private static string GetErrorDescription(UserError error)
        {
            return error switch
            {
                UserError.NoKeys => LocaleManager.Instance["UserErrorNoKeysDescription"],
                UserError.NoFirmware => LocaleManager.Instance["UserErrorNoFirmwareDescription"],
                UserError.FirmwareParsingFailed => LocaleManager.Instance["UserErrorFirmwareParsingFailedDescription"],
                UserError.ApplicationNotFound => LocaleManager.Instance["UserErrorApplicationNotFoundDescription"],
                UserError.Unknown => LocaleManager.Instance["UserErrorUnknownDescription"],
                _ => LocaleManager.Instance["UserErrorUndefinedDescription"]
            };
        }

        private static bool IsCoveredBySetupGuide(UserError error)
        {
            return error switch
            {
                UserError.NoKeys or
                    UserError.NoFirmware or
                    UserError.FirmwareParsingFailed => true,
                _ => false
            };
        }

        private static string GetSetupGuideUrl(UserError error)
        {
            if (!IsCoveredBySetupGuide(error))
            {
                return null;
            }

            return error switch
            {
                UserError.NoKeys => SetupGuideUrl + "#initial-setup---placement-of-prodkeys",
                UserError.NoFirmware => SetupGuideUrl + "#initial-setup-continued---installation-of-firmware",
                _ => SetupGuideUrl
            };
        }

        public static async Task ShowUserErrorDialog(UserError error, StyleableWindow owner)
        {
            string errorCode = GetErrorCode(error);

            bool isInSetupGuide = IsCoveredBySetupGuide(error);

            string setupButtonLabel = isInSetupGuide ? LocaleManager.Instance["OpenSetupGuideMessage"] : "";

            var result = await ContentDialogHelper.CreateInfoDialog(
                string.Format(LocaleManager.Instance["DialogUserErrorDialogMessage"], errorCode, GetErrorTitle(error)),
                GetErrorDescription(error) + (isInSetupGuide
                    ? LocaleManager.Instance["DialogUserErrorDialogInfoMessage"]
                    : ""), setupButtonLabel, LocaleManager.Instance["InputDialogOk"],
                string.Format(LocaleManager.Instance["DialogUserErrorDialogTitle"], errorCode));

            if (result == UserResult.Ok)
            {
                OpenHelper.OpenUrl(GetSetupGuideUrl(error));
            }
        }
    }
}