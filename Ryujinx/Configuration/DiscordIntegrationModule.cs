using DiscordRPC;
using Ryujinx.Common;
using System;
using System.IO;
using System.Linq;

namespace Ryujinx.Configuration
{
    static class DiscordIntegrationModule
    {
        private static DiscordRpcClient DiscordClient;

        private static string LargeDescription = "Ryujinx is a Nintendo Switch emulator.";

        public static RichPresence DiscordPresence { get; private set; }

        public static void Initialize()
        {
            DiscordPresence = new RichPresence
            {
                Assets     = new Assets
                {
                    LargeImageKey  = "ryujinx",
                    LargeImageText = LargeDescription
                },
                Details    = "Main Menu",
                State      = "Idling",
                Timestamps = new Timestamps(DateTime.UtcNow)
            };

            ConfigurationState.Instance.EnableDiscordIntegration.Event += Update;
        }

        private static void Update(object sender, ReactiveEventArgs<bool> e)
        {
            if (e.OldValue != e.NewValue)
            {
                // If the integration was active, disable it and unload everything
                if (e.OldValue)
                {
                    DiscordClient?.Dispose();

                    DiscordClient = null;
                }

                // If we need to activate it and the client isn't active, initialize it
                if (e.NewValue && DiscordClient == null)
                {
                    DiscordClient = new DiscordRpcClient("568815339807309834");

                    DiscordClient.Initialize();
                    DiscordClient.SetPresence(DiscordPresence);
                }
            }
        }

        public static void SwitchToPlayingState(string titleId, string titleName)
        {
            if (File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RPsupported.dat")).Contains(titleId))
            {
                DiscordPresence.Assets.LargeImageKey = titleId;
            }

            string state = titleId;

            if (state == null)
            {
                state = "Ryujinx";
            }
            else
            {
                state = state.ToUpper();
            }

            string details = "Idling";

            if (titleName != null)
            {
                details = $"Playing {titleName}";
            }

            DiscordPresence.Details               = details;
            DiscordPresence.State                 = state;
            DiscordPresence.Assets.LargeImageText = titleName;
            DiscordPresence.Assets.SmallImageKey  = "ryujinx";
            DiscordPresence.Assets.SmallImageText = LargeDescription;
            DiscordPresence.Timestamps            = new Timestamps(DateTime.UtcNow);

            DiscordClient?.SetPresence(DiscordPresence);
        }

        public static void SwitchToMainMenu()
        {
            DiscordPresence.Details               = "Main Menu";
            DiscordPresence.State                 = "Idling";
            DiscordPresence.Assets.LargeImageKey  = "ryujinx";
            DiscordPresence.Assets.LargeImageText = LargeDescription;
            DiscordPresence.Assets.SmallImageKey  = null;
            DiscordPresence.Assets.SmallImageText = null;
            DiscordPresence.Timestamps            = new Timestamps(DateTime.UtcNow);

            DiscordClient?.SetPresence(DiscordPresence);
        }

        public static void Exit()
        {
            DiscordClient?.Dispose();
        }
    }
}
