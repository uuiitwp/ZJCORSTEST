using System;

namespace RTCM3.RTCM3Message
{
    public abstract class RTCM3Base
    {
        private const uint Preamble = 0xd3;
        public static void EncodeRTCM3(ref Memory<byte> result, int bodyBitsLength)
        {
            uint bodyBytesLength = (uint)(bodyBitsLength / 8);
            if (bodyBitsLength % 8 > 0)
            {
                bodyBytesLength++;
            }
            BitOperation.SetBitsUint(ref result, 0, 8, Preamble);
            BitOperation.SetBitsUint(ref result, 14, 10, bodyBytesLength);
            BitOperation.SetBitsUint(ref result, 24 + (int)bodyBytesLength * 8, 24, (uint)CRC24Q.Get(result[..(int)(bodyBytesLength + 3)].Span));

        }
        public abstract Memory<byte> Encode();
    }
}
