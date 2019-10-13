namespace Ryujinx.Graphics.Gpu.Image
{
    struct TextureDescriptor
    {
        public uint Word0;
        public uint Word1;
        public uint Word2;
        public uint Word3;
        public uint Word4;
        public uint Word5;
        public uint Word6;
        public uint Word7;

        public uint UnpackFormat()
        {
            return Word0 & 0x8007ffff;
        }

        public TextureComponent UnpackSwizzleR()
        {
            return(TextureComponent)((Word0 >> 19) & 7);
        }

        public TextureComponent UnpackSwizzleG()
        {
            return(TextureComponent)((Word0 >> 22) & 7);
        }

        public TextureComponent UnpackSwizzleB()
        {
            return(TextureComponent)((Word0 >> 25) & 7);
        }

        public TextureComponent UnpackSwizzleA()
        {
            return(TextureComponent)((Word0 >> 28) & 7);
        }

        public ulong UnpackAddress()
        {
            return Word1 | ((ulong)(Word2 & 0xffff) << 32);
        }

        public TextureDescriptorType UnpackTextureDescriptorType()
        {
            return (TextureDescriptorType)((Word2 >> 21) & 7);
        }

        public int UnpackStride()
        {
            return (int)(Word3 & 0xffff) << 5;
        }

        public int UnpackGobBlocksInX()
        {
            return 1 << (int)(Word3 & 7);
        }

        public int UnpackGobBlocksInY()
        {
            return 1 << (int)((Word3 >> 3) & 7);
        }

        public int UnpackGobBlocksInZ()
        {
            return 1 << (int)((Word3 >> 6) & 7);
        }

        public int UnpackGobBlocksInTileX()
        {
            return 1 << (int)((Word3 >> 10) & 7);
        }

        public int UnpackLevels()
        {
            return (int)(Word3 >> 28) + 1;
        }

        public int UnpackWidth()
        {
            return (int)(Word4 & 0xffff) + 1;
        }

        public bool UnpackSrgb()
        {
            return (Word4 & (1 << 22)) != 0;
        }

        public TextureTarget UnpackTextureTarget()
        {
            return (TextureTarget)((Word4 >> 23) & 0xf);
        }

        public int UnpackHeight()
        {
            return (int)(Word5 & 0xffff) + 1;
        }

        public int UnpackDepth()
        {
            return (int)((Word5 >> 16) & 0x3fff) + 1;
        }

        public int UnpackBaseLevel()
        {
            return (int)(Word7 & 0xf);
        }

        public int UnpackMaxLevelInclusive()
        {
            return (int)((Word7 >> 4) & 0xf);
        }

        public TextureMsaaMode UnpackTextureMsaaMode()
        {
            return (TextureMsaaMode)((Word7 >> 8) & 0xf);
        }
    }
}
