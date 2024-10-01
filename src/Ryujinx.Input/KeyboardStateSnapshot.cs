using System.Runtime.CompilerServices;

namespace Ryujinx.Input
{
    /// <summary>
    /// A snapshot of a <see cref="IKeyboard"/>.
    /// </summary>
    public class KeyboardStateSnapshot
    {
        private readonly bool[] _keysState;

        /// <summary>
        /// Create a new <see cref="KeyboardStateSnapshot"/>.
        /// </summary>
        /// <param name="keysState">The keys state</param>
        public KeyboardStateSnapshot(bool[] keysState)
        {
            _keysState = keysState;
        }

        /// <summary>
        /// Check if a given key is pressed.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>True if the given key is pressed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPressed(Key key) => _keysState[(int)key];
    }
}
