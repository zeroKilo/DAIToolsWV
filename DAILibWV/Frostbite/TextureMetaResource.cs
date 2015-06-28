using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAILibWV.Frostbite
{
    public class TextureMetaResource
    {
        public struct DDSPixelFormat
        {
            public int dwSize;
            public int dwFlags;
            public int dwFourCC;
            public int dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;
            public uint caps2;
        }

        public byte[] data;
        public int formatID;
        public ushort width;
        public ushort height;
        public int mipCount;
        public List<int> mipDataSizes;
        public byte[] chunkid;
        public DDSPixelFormat ddsPF;

        public TextureMetaResource(byte[] buff)
        {
            data = buff;
            ReadData();
        }

        private void ReadData()
        {
            MemoryStream m = new MemoryStream(data);
            m.Seek(12, 0);
            formatID = Helpers.ReadInt(m);
            m.Seek(2, SeekOrigin.Current);
            width = Helpers.ReadUShort(m);
            height = Helpers.ReadUShort(m);
            m.Seek(4, SeekOrigin.Current);
            mipCount = m.ReadByte();
            m.Seek(1, SeekOrigin.Current);
            chunkid = new byte[16];
            m.Read(chunkid, 0, 16);
            mipDataSizes = new List<int>();
            for (int i = 0; i < Math.Min(mipCount, 14); i++)
                mipDataSizes.Add(Helpers.ReadInt(m));
            SetPixelFormatData(formatID);
        }

        public static Dictionary<int, int> PixelFormatTypes = new Dictionary<int, int>()
        {
            { 0x00, 0x31545844 },
            { 0x01, 0x31545844 },
            { 0x03, 0x35545844 },
            { 0x04, 0x31495441 },
            { 0x10, 0x74 },
            { 0x13, 0x32495441 },
            { 0x14, 0x53354342 },
        };

        private void SetPixelFormatData(int pixelFormatID)
        {
            ddsPF = new DDSPixelFormat();
            ddsPF.caps2 = 0;
            ddsPF.dwSize = 32;
            ddsPF.dwFlags = 4;
            ddsPF.dwFourCC = 0x31545844;
            ddsPF.dwRGBBitCount = 0;
            ddsPF.dwRBitMask = 0;
            ddsPF.dwGBitMask = 0;
            ddsPF.dwBBitMask = 0;
            ddsPF.dwABitMask = 0;
            if (PixelFormatTypes.ContainsKey(pixelFormatID))
            {
                ddsPF.dwFourCC = PixelFormatTypes[pixelFormatID];
                if (pixelFormatID == 0x01)
                    ddsPF.dwFlags |= 0x01;
            }
            else
            {
                switch (pixelFormatID)
                {
                    case 0x0B:
                    case 0x36:
                        ddsPF.dwFourCC = 0x00;
                        ddsPF.dwRGBBitCount = 0x20;
                        ddsPF.dwRBitMask = 0xFF;
                        ddsPF.dwGBitMask = 0xFF00;
                        ddsPF.dwBBitMask = 0xFF0000;
                        ddsPF.dwABitMask = 0xFF000000;
                        ddsPF.dwFlags = 0x41;
                        if (pixelFormatID == 0x36)
                            ddsPF.caps2 = 0xFE00;
                        break;
                    case 0x0C:
                        ddsPF.dwFourCC = 0x00;
                        ddsPF.dwRGBBitCount = 0x08;
                        ddsPF.dwABitMask = 0xFF;
                        ddsPF.dwFlags = 0x02;
                        break;
                    case 0x0D:
                        ddsPF.dwFourCC = 0x00;
                        ddsPF.dwRGBBitCount = 0x10;
                        ddsPF.dwRBitMask = 0xFFFF;
                        ddsPF.dwFlags = 0x20000;
                        break;
                }
            }
        }

        public void WriteTextureHeader(Stream s)
        {
            Helpers.WriteInt(s, 0x20534444);
            Helpers.WriteInt(s, 124);
            Helpers.WriteInt(s, 0x000A1007);
            Helpers.WriteInt(s, height);
            Helpers.WriteInt(s, width);
            Helpers.WriteInt(s, mipDataSizes[0]);
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, mipCount);
            for (int i = 0; i < 11; i++)
                Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, ddsPF.dwSize);
            Helpers.WriteInt(s, ddsPF.dwFlags);
            Helpers.WriteInt(s, ddsPF.dwFourCC);
            Helpers.WriteInt(s, ddsPF.dwRGBBitCount);
            Helpers.WriteInt(s, (int)ddsPF.dwRBitMask);
            Helpers.WriteInt(s, (int)ddsPF.dwGBitMask);
            Helpers.WriteInt(s, (int)ddsPF.dwBBitMask);
            Helpers.WriteInt(s, (int)ddsPF.dwABitMask);
            Helpers.WriteInt(s,0);
            Helpers.WriteInt(s, (int)ddsPF.caps2);
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
        }
    }
}
