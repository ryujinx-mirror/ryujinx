using System.Runtime.CompilerServices;

namespace Ryujinx.Input
{
    /// <summary>
    /// Represent an emulated keyboard.
    /// </summary>
    public interface IKeyboard : IGamepad
    {
        /// <summary>
        /// Check if a given key is pressed on the keyboard.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>True if the given key is pressed on the keyboard</returns>
        bool IsPressed(Key key);

        /// <summary>
        /// Get a snaphost of the state of the keyboard.
        /// </summary>
        /// <returns>A snaphost of the state of the keyboard.</returns>
        KeyboardStateSnapshot GetKeyboardStateSnapshot();

        /// <summary>
        /// Get a snaphost of the state of a keyboard.
        /// </summary>
        /// <param name="keyboard">The keyboard to do a snapshot of</param>
        /// <returns>A snaphost of the state of the keyboard.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static KeyboardStateSnapshot GetStateSnapshot(IKeyboard keyboard)
        {
            bool[] keysState = new bool[(int)Key.Count];

            for (Key key = 0; key < Key.Count; key++)
            {
                keysState[(int)key] = keyboard.IsPressed(key);
            }

            return new KeyboardStateSnapshot(keysState);
        }
    }
}
