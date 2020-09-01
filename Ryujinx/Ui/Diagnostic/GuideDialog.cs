using Gtk;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ryujinx.Ui.Diagnostic
{
    internal class GuideDialog : MessageDialog
    {
        internal static bool _isExitDialogOpen = false;

        public GuideDialog(string title, string mainText, string secondaryText) : base(null, DialogFlags.Modal, MessageType.Other, ButtonsType.None, null)
        {
            Title = title;
            Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png");
            Text = mainText;
            SecondaryText = secondaryText;
            WindowPosition = WindowPosition.Center;
            Response += GtkDialog_Response;

            Button guideButton = new Button();
            guideButton.Label = "Open the Setup Guide";

            ContentArea.Add(guideButton);

            SetSizeRequest(100, 10);
            ShowAll();
        }

        private void GtkDialog_Response(object sender, ResponseArgs args)
        {
            Dispose();
        }
    }
}
