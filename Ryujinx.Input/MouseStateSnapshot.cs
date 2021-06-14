using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Input
{
    /// <summary>
    /// A snapshot of a <see cref="IMouse"/>.
    /// </summary>
    public class MouseStateSnapshot
    {
        private bool[] _buttonState;

        public Vector2 Position { get; }

        /// <summary>
        /// Create a new <see cref="MouseStateSnapshot"/>.
        /// </summary>
        /// <param name="buttonState">The keys state</param>
        public MouseStateSnapshot(bool[] buttonState, Vector2 position)
        {
            _buttonState = buttonState;

            Position = position;
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