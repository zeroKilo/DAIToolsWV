using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAILibWV
{
    public static class HalfUtils
    {
        private static readonly ushort[] FloatToHalfBaseTable;
        private static readonly byte[] FloatToHalfShiftTable;
        private static readonly int[] HalfToFloatExponentTable;
        private static readonly uint[] HalfToFloatMantissaTable;
        private static readonly uint[] HalfToFloatOffsetTable;

        static HalfUtils()
        {
            int num;
            HalfToFloatMantissaTable = new uint[0x800];
            HalfToFloatExponentTable = new int[0x40];
            HalfToFloatOffsetTable = new uint[0x40];
            FloatToHalfBaseTable = new ushort[0x200];
            FloatToHalfShiftTable = new byte[0x200];
            HalfToFloatMantissaTable[0] = 0;
            for (num = 1; num < 0x400; num++)
            {
                uint num2 = (uint)(num << 13);
                uint num3 = 0;
                while ((num2 & 0x800000) == 0)
                {
                    num3 -= 0x800000;
                    num2 = num2 << 1;
                }
                num2 &= 0xff7fffff;
                num3 += 0x38800000;
                HalfToFloatMantissaTable[num] = num2 | num3;
            }
            for (num = 0x400; num < 0x800; num++)
            {
                HalfToFloatMantissaTable[num] = (uint)(0x38000000 + ((num - 0x400) << 13));
            }
            HalfToFloatExponentTable[0] = 0;
            for (num = 1; num < 0x3f; num++)
            {
                if (num >= 0x1f)
                {
                    HalfToFloatExponentTable[num] = -2147483648 + ((num - 0x20) << 0x17);
                }
                else
                {
                    HalfToFloatExponentTable[num] = num << 0x17;
                }
            }
            HalfToFloatExponentTable[0x1f] = 0x47800000;
            HalfToFloatExponentTable[0x20] = -2147483648;
            HalfToFloatExponentTable[0x3f] = -947912704;
            HalfToFloatOffsetTable[0] = 0;
            for (num = 1; num < 0x40; num++)
            {
                HalfToFloatOffsetTable[num] = 0x400;
            }
            HalfToFloatOffsetTable[0x20] = 0;
            for (num = 0; num < 0x100; num++)
            {
                int num4 = num - 0x7f;
                if (num4 < -24)
                {
                    FloatToHalfBaseTable[num] = 0;
                    FloatToHalfBaseTable[num | 0x100] = 0x8000;
                    FloatToHalfShiftTable[num] = 0x18;
                    FloatToHalfShiftTable[num | 0x100] = 0x18;
                }
                else if (num4 < -14)
                {
                    FloatToHalfBaseTable[num] = (ushort)(((int)0x400) >> (-num4 - 14));
                    FloatToHalfBaseTable[num | 0x100] = (ushort)((((int)0x400) >> (-num4 - 14)) | 0x8000);
                    FloatToHalfShiftTable[num] = Convert.ToByte((int)(-num4 - 1));
                    FloatToHalfShiftTable[num | 0x100] = Convert.ToByte((int)(-num4 - 1));
                }
                else if (num4 <= 15)
                {
                    FloatToHalfBaseTable[num] = (ushort)((num4 + 15) << 10);
                    FloatToHalfBaseTable[num | 0x100] = (ushort)(((num4 + 15) << 10) | 0x8000);
                    FloatToHalfShiftTable[num] = 13;
                    FloatToHalfShiftTable[num | 0x100] = 13;
                }
                else if (num4 >= 0x80)
                {
                    FloatToHalfBaseTable[num] = 0x7c00;
                    FloatToHalfBaseTable[num | 0x100] = 0xfc00;
                    FloatToHalfShiftTable[num] = 13;
                    FloatToHalfShiftTable[num | 0x100] = 13;
                }
                else
                {
                    FloatToHalfBaseTable[num] = 0x7c00;
                    FloatToHalfBaseTable[num | 0x100] = 0xfc00;
                    FloatToHalfShiftTable[num] = 0x18;
                    FloatToHalfShiftTable[num | 0x100] = 0x18;
                }
            }
        }

        public static ushort Pack(float f)
        {
            FloatToUint num = new FloatToUint
            {
                floatValue = f
            };
            return (ushort)(FloatToHalfBaseTable[((int)(num.uintValue >> 0x17)) & 0x1ff] + ((num.uintValue & 0x7fffff) >> (FloatToHalfShiftTable[((int)(num.uintValue >> 0x17)) & 0x1ff] & 0x1f)));
        }

        public static float Unpack(ushort h)
        {
            FloatToUint num = new FloatToUint
            {
                uintValue = HalfToFloatMantissaTable[((int)HalfToFloatOffsetTable[h >> 10]) + (h & 0x3ff)] + ((uint)HalfToFloatExponentTable[h >> 10])
            };
            return num.floatValue;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatToUint
        {
            [FieldOffset(0)]
            public float floatValue;
            [FieldOffset(0)]
            public uint uintValue;
        }
    }
}
