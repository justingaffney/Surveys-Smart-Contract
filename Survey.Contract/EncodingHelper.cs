using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survey.Contract
{
    public static class EncodingHelper
    {
        public static byte[] AsByteArray(this int value)
        {
            return new byte[]
            {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 24) & 0xFF)
            };
        }

        public static byte[] AsByteArray(this uint value)
        {
            return new byte[]
            {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 24) & 0xFF)
            };
        }
    }
}