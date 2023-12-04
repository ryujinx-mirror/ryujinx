namespace Ryujinx.Input
{
    /// <summary>
    /// Represent a button from a gamepad.
    /// </summary>
    public enum GamepadButtonInputId : byte
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
