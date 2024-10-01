using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Input
{
    /// <summary>
    /// A snapshot of a <see cref="IMouse"/>.
    /// </summary>
    public class MouseStateSnapshot
    {
        private readonly bool[] _buttonState;

        /// <summary>
        /// The position of the mouse cursor
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        /// The scroll delta of the mouse
        /// </summary>
        public Vector2 Scroll { get; }

        /// <summary>
        /// Create a new <see cref="MouseStateSnapshot"/>.
        /// </summary>
        /// <param name="buttonState">The button state</param>
        /// <param name="position">The position of the cursor</param>
        /// <param name="scroll">The scroll delta</param>
        public MouseStateSnapshot(bool[] buttonState, Vector2 position, Vector2 scroll)
        {
            _buttonState = buttonState;

            Position = position;
            Scroll = scroll;
        }

        /// <summary>
        /// Check if a given button is pressed.
        /// </summary>
        /// <param name="button">The button</param>
        /// <returns>True if the given button is pressed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPressed(MouseButton button) => _buttonState[(int)button];
    }
}
