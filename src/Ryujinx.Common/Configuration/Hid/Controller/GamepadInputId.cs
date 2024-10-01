using Ryujinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Ryujinx.Common.Configuration.Hid.Controller
{
    [JsonConverter(typeof(TypedStringEnumConverter<GamepadInputId>))]
    public enum GamepadInputId : byte
    {
        Unbound,
        A,
        B,
        X,
        Y,
        LeftStick,
        RightStick,
        LeftShoulder,
        RightShoulder,

        // Likely axis
        LeftTrigger,
        // Likely axis
        RightTrigger,

        DpadUp,
        DpadDown,
        DpadLeft,
        DpadRight,

        // Special buttons

        Minus,
        Plus,

        Back = Minus,
        Start = Plus,

        Guide,
        Misc1,

        // Xbox Elite paddle
        Paddle1,
        Paddle2,
        Paddle3,
        Paddle4,

        // PS5 touchpad button
        Touchpad,

        // Virtual buttons for single joycon
        SingleLeftTrigger0,
        SingleRightTrigger0,

        SingleLeftTrigger1,
        SingleRightTrigger1,

        Count,
    }
}
