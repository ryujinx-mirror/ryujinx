using DiscordRPC;
using Ryujinx.Common;
using Ryujinx.Configuration;
using System;
using System.Linq;

namespace Ryujinx.Modules
{
    static class DiscordIntegrationModule
    {
        private static DiscordRpcClient _discordClient;

        private const string LargeDescription = "Ryujinx is a Nintendo Switch emulator.";

        public static RichPresence DiscordPresence { get; private set; }

        public static void Initialize()
        {
            DiscordPresence = new RichPresence
            {
                Assets = new Assets
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
                    _discordClient?.Dispose();

                    _discordClient = null;
                }

                // If we need to activate it and the client isn't active, initialize it
                if (e.NewValue && _discordClient == null)
                {
                    _discordClient = new DiscordRpcClient("568815339807309834");

                    _discordClient.Initialize();
                    _discordClient.SetPresence(DiscordPresence);
                }
            }
        }

        public static void SwitchToPlayingState(string titleId, string titleName)
        {
            if (SupportedTitles.Contains(titleId))
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

            _discordClient?.SetPresence(DiscordPresence);
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

            _discordClient?.SetPresence(DiscordPresence);
        }

        public static void Exit()
        {
            _discordClient?.Dispose();
        }

        private static readonly string[] SupportedTitles =
        {
            "0100000000010000", // Super Mario Odyssey™
            "01000b900d8b0000", // Cadence of Hyrule – Crypt of the NecroDancer Featuring The Legend of Zelda
            "01000d200ac0c000", // Bud Spencer & Terence Hill - Slaps And Beans
            "01000d700be88000", // My Girlfriend is a Mermaid!?
            "01000dc007e90000", // Sparkle Unleashed
            "01000e2003fa0000", // MIGHTY GUNVOLT BURST
            "0100225000fee000", // Blaster Master Zero
            "010028d0045ce000", // Sparkle 2
            "01002b30028f6000", // Celeste
            "01002fc00c6d0000", // Witch Thief
            "010034e005c9c000", // Code of Princess EX
            "010036b0034e4000", // Super Mario Party™
            "01003d200baa2000", // Pokémon Mystery Dungeon™: Rescue Team DX
            "01004f8006a78000", // Super Meat Boy
            "010051f00ac5e000", // SEGA AGES Sonic The Hedgehog
            "010055d009f78000", // Fire Emblem™: Three Houses
            "010056e00853a000", // A Hat in Time
            "0100574009f9e000", // 嘘つき姫と盲目王子
            "01005d700e742000", // DOOM 64
            "0100628004bce000", // Nights of Azure 2: Bride of the New Moon
            "0100633007d48000", // Hollow Knight
            "010065500b218000", // メモリーズオフ -Innocent Fille-
            "010068f00aa78000", // FINAL FANTASY XV POCKET EDITION HD
            "01006bb00c6f0000", // The Legend of Zelda™: Link’s Awakening
            "01006f8002326000", // Animal Crossing™: New Horizons
            "01006a800016e000", // Super Smash Bros.™ Ultimate
            "010072800cbe8000", // PC Building Simulator
            "01007300020fa000", // ASTRAL CHAIN
            "01007330027ee000", // Ultra Street Fighter® II: The Final Challengers
            "0100749009844000", // 20XX
            "01007a4008486000", // Enchanting Mahjong Match
            "01007ef00011e000", // The Legend of Zelda™: Breath of the Wild
            "010080b00ad66000", // Undertale
            "010082400bcc6000", // Untitled Goose Game
            "01008db008c2c000", // Pokémon™ Shield
            "010094e00b52e000", // Capcom Beat 'Em Up Bundle
            "01009aa000faa000", // Sonic Mania
            "01009b90006dc000", // Super Mario Maker™ 2
            "01009cc00c97c000", // DEAD OR ALIVE Xtreme 3 Scarlet 基本無料版
            "0100ea80032ea000", // New Super Mario Bros.™ U Deluxe
            "0100a4200a284000", // LUMINES REMASTERED
            "0100a5c00d162000", // Cuphead
            "0100abf008968000", // Pokémon™ Sword
            "0100ae000aebc000", // Angels of Death
            "0100b3f000be2000", // Pokkén Tournament™ DX
            "0100bc2004ff4000", // Owlboy
            "0100cf3007578000", // Atari Flashback Classics
            "0100d5d00c6be000", // Our World Is Ended.
            "0100d6b00cd88000", // YUMENIKKI -DREAM DIARY-
            "0100d870045b6000", // Nintendo Entertainment System™ - Nintendo Switch Online
            "0100e0c00adac000", // SENRAN KAGURA Reflexions
            "0100e46006708000", // Terraria
            "0100e7200b272000", // Lanota
            "0100e9f00b882000", // null
            "0100eab00605c000", // Poly Bridge
            "0100efd00a4fa000", // Shantae and the Pirate's Curse
            "0100f6a00a684000", // ひぐらしのなく頃に奉
            "0100f9f00c696000", // Crash™ Team Racing Nitro-Fueled
            "051337133769a000", // RGB-Seizure
        };
    }
}