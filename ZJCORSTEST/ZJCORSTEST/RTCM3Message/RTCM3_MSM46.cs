using System;

namespace RTCM3.RTCM3Message
{
    public class RTCM3_MSM46 : RTCM3_MSM
    {
        public uint[] Range;
        public uint[] RangeM;
        public RTCM3_MSM46(ReadOnlySpan<byte> databody) : base(databody)
        {
            Range = new uint[SatNumber];
            for (int j = 0; j < Range.Length; j++)
            {
                uint tmp = BitOperation.GetBitsUint(databody, i, 8);
                i += 8;
                Range[j] = tmp;

            }
            RangeM = new uint[SatNumber];
            for (int j = 0; j < RangeM.Length; j++)
            {
                uint tmp = BitOperation.GetBitsUint(databody, i, 10);
                i += 10;
                RangeM[j] = tmp;
            }
        }

        public void EncodeSatData(ref Memory<byte> bytes)
        {
            int i = 24 + 169 + Cell.Length;
            for (int j = 0; j < Range.Length; j++)
            {
                BitOperation.SetBitsUint(ref bytes, i, 8, Range[j]);
                i += 8;
            }
            for (int j = 0; j < RangeM.Length; j++)
            {
                BitOperation.SetBitsUint(ref bytes, i, 10, RangeM[j]);
                i += 10;
            }
        }
    }
}
