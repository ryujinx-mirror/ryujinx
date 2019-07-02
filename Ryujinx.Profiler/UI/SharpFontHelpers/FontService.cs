using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpFont;

namespace Ryujinx.Profiler.UI.SharpFontHelpers
{
    public class FontService
    {
        private struct CharacterInfo
        {
            public float Left;
            public float Right;
            public float Top;
            public float Bottom;

            public int Width;
            public float Height;

            public float AspectRatio;

            public float BearingX;
            public float BearingY;
            public float Advance;
        }

        private const int SheetWidth  = 1024;
        private const int SheetHeight = 512;
        private int ScreenWidth, ScreenHeight;
        private int CharacterTextureSheet;
        private CharacterInfo[] characters;

        public Color fontColor { get; set; } = Color.Black;

        private string GetFontPath()
        {
            string fontFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            // Only uses Arial, add more fonts here if wanted
            string path = Path.Combine(fontFolder, "arial.ttf");
            if (File.Exists(path))
            {
                return path;
            }

            throw new Exception($"Profiler exception. Required font Courier New or Arial not installed to {fontFolder}");
        }

        public void InitializeTextures()
        {
            // Create and init some vars
            uint[] rawCharacterSheet = new uint[SheetWidth * SheetHeight];
            int x;
            int y;
            int lineOffset;
            int maxHeight;

            x = y = lineOffset = maxHeight = 0;
            characters = new CharacterInfo[94];

            // Get font
            var font = new FontFace(File.OpenRead(GetFontPath()));

            // Update raw data for each character
            for (int i = 0; i < 94; i++)
            {
                var surface = RenderSurface((char)(i + 33), font, out float xBearing, out float yBearing, out float advance);

                characters[i] = UpdateTexture(surface, ref rawCharacterSheet, ref x, ref y, ref lineOffset);
                characters[i].BearingX = xBearing;
                characters[i].BearingY = yBearing;
                characters[i].Advance  = advance;

                if (maxHeight < characters[i].Height)
                    maxHeight = (int)characters[i].Height;
            }

            // Fix height for characters shorter than line height
            for (int i = 0; i < 94; i++)
            {
                characters[i].BearingX   /= characters[i].Width;
                characters[i].BearingY   /= maxHeight;
                characters[i].Advance    /= characters[i].Width;
                characters[i].Height     /= maxHeight;
                characters[i].AspectRatio = (float)characters[i].Width / maxHeight;
            }

            // Convert raw data into texture
            CharacterTextureSheet = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, CharacterTextureSheet);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,     (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,     (int)TextureWrapMode.Clamp);
            
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, SheetWidth, SheetHeight, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, rawCharacterSheet);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void UpdateScreenHeight(int height)
        {
            ScreenHeight = height;
        }

        public float DrawText(string text, float x, float y, float height, bool draw = true)
        {
            float originalX = x;

            // Skip out of bounds draw
            if (y < height * -2 || y > ScreenHeight + height * 2)
            {
                draw = false;
            }

            if (draw)
            {
                // Use font map texture
                GL.BindTexture(TextureTarget.Texture2D, CharacterTextureSheet);

                // Enable blending and textures
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                // Draw all characters
                GL.Begin(PrimitiveType.Triangles);
                GL.Color4(fontColor);
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                {
                    x += height / 4;
                    continue;
                }

                CharacterInfo charInfo = characters[text[i] - 33];
                float width = (charInfo.AspectRatio * height);
                x += (charInfo.BearingX * charInfo.AspectRatio) * width;
                float right = x + width;
                if (draw)
                {
                    DrawChar(charInfo, x, right, y + height * (charInfo.Height - charInfo.BearingY), y - height * charInfo.BearingY);
                }
                x = right + charInfo.Advance * charInfo.AspectRatio + 1;
            }

            if (draw)
            {
                GL.End();

                // Cleanup for caller
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.Texture2D);
                GL.Disable(EnableCap.Blend);
            }

            // Return width of rendered text
            return x - originalX;
        }

        private void DrawChar(CharacterInfo charInfo, float left, float right, float top, float bottom)
        {
            GL.TexCoord2(charInfo.Left, charInfo.Bottom);  GL.Vertex2(left, bottom);
            GL.TexCoord2(charInfo.Left, charInfo.Top);     GL.Vertex2(left, top);
            GL.TexCoord2(charInfo.Right, charInfo.Top);    GL.Vertex2(right, top);

            GL.TexCoord2(charInfo.Right, charInfo.Top);    GL.Vertex2(right, top);
            GL.TexCoord2(charInfo.Right, charInfo.Bottom); GL.Vertex2(right, bottom);
            GL.TexCoord2(charInfo.Left, charInfo.Bottom);  GL.Vertex2(left, bottom);
        }

        public unsafe Surface RenderSurface(char c, FontFace font, out float xBearing, out float yBearing, out float advance)
        {
            var glyph = font.GetGlyph(c, 64);
            xBearing  = glyph.HorizontalMetrics.Bearing.X;
            yBearing  = glyph.RenderHeight - glyph.HorizontalMetrics.Bearing.Y;
            advance   = glyph.HorizontalMetrics.Advance;

            var surface = new Surface
            {
                Bits   = Marshal.AllocHGlobal(glyph.RenderWidth * glyph.RenderHeight),
                Width  = glyph.RenderWidth,
                Height = glyph.RenderHeight,
                Pitch  = glyph.RenderWidth
            };

            var stuff = (byte*)surface.Bits;
            for (int i = 0; i < surface.Width * surface.Height; i++)
                *stuff++ = 0;

            glyph.RenderTo(surface);

            return surface;
        }

        private CharacterInfo UpdateTexture(Surface surface, ref uint[] rawCharMap, ref int posX, ref int posY, ref int lineOffset)
        {
            int width   = surface.Width;
            int height  = surface.Height;
            int len     = width * height;
            byte[] data = new byte[len];

            // Get character bitmap
            Marshal.Copy(surface.Bits, data, 0, len);

            // Find a slot
            if (posX + width > SheetWidth)
            {
                posX       = 0;
                posY      += lineOffset;
                lineOffset = 0;
            }

            // Update lineOffset
            if (lineOffset < height)
            {
                lineOffset = height + 1;
            }

            // Copy char to sheet
            for (int y = 0; y < height; y++)
            {
                int destOffset   = (y + posY) * SheetWidth + posX;
                int sourceOffset = y * width;

                for (int x = 0; x < width; x++)
                {
                    rawCharMap[destOffset + x] = (uint)((0xFFFFFF << 8) | data[sourceOffset + x]);
                }
            }

            // Generate character info
            CharacterInfo charInfo = new CharacterInfo()
            {
                Left   = (float)posX / SheetWidth,
                Right  = (float)(posX + width) / SheetWidth,
                Top    = (float)(posY - 1) / SheetHeight,
                Bottom = (float)(posY + height) / SheetHeight,
                Width  = width,
                Height = height,
            };

            // Update x
            posX += width + 1;

            // Give the memory back
            Marshal.FreeHGlobal(surface.Bits);
            return charInfo;
        }
    }
}