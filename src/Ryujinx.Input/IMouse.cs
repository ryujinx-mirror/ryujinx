using System.Drawing;
using System.Numerics;

namespace Ryujinx.Input
{
    /// <summary>
    /// Represent an emulated mouse.
    /// </summary>
    public interface IMouse : IGamepad
    {
#pragma warning disable IDE0051 // Remove unused private member
        private const int SwitchPanelWidth = 1280;
#pragma warning restore IDE0051
        private const int SwitchPanelHeight = 720;

        /// <summary>
        /// Check if a given button is pressed on the mouse.
        /// </summary>
        /// <param name="button">The button</param>
        /// <returns>True if the given button is pressed on the mouse</returns>
        bool IsButtonPressed(MouseButton button);

        /// <summary>
        /// Get the position of the mouse in the client.
        /// </summary>
        Vector2 GetPosition();

        /// <summary>
        /// Get the mouse scroll delta.
        /// </summary>
        Vector2 GetScroll();

        /// <summary>
        /// Get the client size.
        /// </summary>
        Size ClientSize { get; }

        /// <summary>
        /// Get the button states of the mouse.
        /// </summary>
        bool[] Buttons { get; }

        /// <summary>
        /// Get a snaphost of the state of a mouse.
        /// </summary>
        /// <param name="mouse">The mouse to do a snapshot of</param>
        /// <returns>A snaphost of the state of the mouse.</returns>
        public static MouseStateSnapshot GetMouseStateSnapshot(IMouse mouse)
        {
            bool[] buttons = new bool[(int)MouseButton.Count];

            mouse.Buttons.CopyTo(buttons, 0);

            return new MouseStateSnapshot(buttons, mouse.GetPosition(), mouse.GetScroll());
        }

        /// <summary>
        /// Get the position of a mouse on screen relative to the app's view
        /// </summary>
        /// <param name="mousePosition">The position of the mouse in the client</param>
        /// <param name="clientSize">The size of the client</param>
        /// <param name="aspectRatio">The aspect ratio of the view</param>
        /// <returns>A snaphost of the state of the mouse.</returns>
        public static Vector2 GetScreenPosition(Vector2 mousePosition, Size clientSize, float aspectRatio)
        {
            float mouseX = mousePosition.X;
            float mouseY = mousePosition.Y;

            float aspectWidth = SwitchPanelHeight * aspectRatio;

            int screenWidth = clientSize.Width;
            int screenHeight = clientSize.Height;

            if (clientSize.Width > clientSize.Height * aspectWidth / SwitchPanelHeight)
            {
                screenWidth = (int)(clientSize.Height * aspectWidth) / SwitchPanelHeight;
            }
            else
            {
                screenHeight = (clientSize.Width * SwitchPanelHeight) / (int)aspectWidth;
            }

            int startX = (clientSize.Width - screenWidth) >> 1;
            int startY = (clientSize.Height - screenHeight) >> 1;

            int endX = startX + screenWidth;
            int endY = startY + screenHeight;

            if (mouseX >= startX &&
                mouseY >= startY &&
                mouseX < endX &&
                mouseY < endY)
            {
                int screenMouseX = (int)mouseX - startX;
                int screenMouseY = (int)mouseY - startY;

                mouseX = (screenMouseX * (int)aspectWidth) / screenWidth;
                mouseY = (screenMouseY * SwitchPanelHeight) / screenHeight;

                return new Vector2(mouseX, mouseY);
            }

            return new Vector2();
        }
    }
}
