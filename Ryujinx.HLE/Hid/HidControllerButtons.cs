using System;

namespace Ryujinx.HLE.Input
{
    [Flags]
    public enum HidControllerButtons
    {
        KEY_A            = (1 << 0),
        KEY_B            = (1 << 1),
        KEY_X            = (1 << 2),
        KEY_Y            = (1 << 3),
        KEY_LSTICK       = (1 << 4),
        KEY_RSTICK       = (1 << 5),
        KEY_L            = (1 << 6),
        KEY_R            = (1 << 7),
        KEY_ZL           = (1 << 8),
        KEY_ZR           = (1 << 9),
        KEY_PLUS         = (1 << 10),
        KEY_MINUS        = (1 << 11),
        KEY_DLEFT        = (1 << 12),
        KEY_DUP          = (1 << 13),
        KEY_DRIGHT       = (1 << 14),
        KEY_DDOWN        = (1 << 15),
        KEY_LSTICK_LEFT  = (1 << 16),
        KEY_LSTICK_UP    = (1 << 17),
        KEY_LSTICK_RIGHT = (1 << 18),
        KEY_LSTICK_DOWN  = (1 << 19),
        KEY_RSTICK_LEFT  = (1 << 20),
        KEY_RSTICK_UP    = (1 << 21),
        KEY_RSTICK_RIGHT = (1 << 22),
        KEY_RSTICK_DOWN  = (1 << 23),
        KEY_SL           = (1 << 24),
        KEY_SR           = (1 << 25)
    }
}