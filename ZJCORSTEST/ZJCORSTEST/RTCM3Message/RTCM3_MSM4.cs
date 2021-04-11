using System;

namespace RTCM3.RTCM3Message
{
    public class RTCM3_MSM4 : RTCM3_MSM46
    {
        public int[] prv;
        public int[] cpv;
        public uint[] plock;
        public uint[] half;
        public uint[] cnr;
        public RTCM3_MSM4(ReadOnlySpan<byte> databody) : base(databody)
        {
            prv = new int[NCell];
            for (int j = 0; j < NCell; j++)
            {
                int temp = BitOperation.GetBitsInt(databody, i, 15);
                prv[j] = temp;
                i += 15;
            }
            cpv = new int[NCell];
            for (int j = 0; j < NCell; j++)
            {

                int temp = BitOperation.GetBitsInt(databody, i, 22);
                cpv[j] = temp;
                i += 22;


            }
            plock = new uint[NCell];
            for (int j = 0; j < NCell; j++)
            {
                uint temp = BitOperation.GetBitsUint(databody, i, 4);
                plock[j] = temp;
                i += 4;
            }
            half = new uint[NCell];
            for (int j = 0; j < NCell; j++)
            {
                uint temp = BitOperation.GetBitsUint(databody, i, 1);
                half[j] = temp;
                i += 1;
            }
            cnr = new uint[NCell];
            for (int j = 0; j < NCell; j++)
            {
                uint temp = BitOperation.GetBitsUint(databody, i, 6);
                cnr[j] = temp;
                i += 6;
            }
        }
        public override Memory<byte> Encode()
        {
            int i = (int)(24 + 169 + Cell.Length + 18 * SatNumber);
            int bitLength = (int)(i + 24 + 48 * NCell);
            int byteLength = bitLength / 8;
            if (bitLength % 8 != 0)
            {
                byteLength++;
            }
            Memory<byte> result = new Memory<byte>(new byte[byteLength]);
            EncodeMSMHeader(ref result);
            EncodeSatData(ref result);
            for (int j = 0; j < NCell; j++)
            {
                BitOperation.SetBitsInt(ref result, i, 15, prv[j]);
                i += 15;
            }
            for (int j = 0; j < NCell; j++)
            {
                BitOperation.SetBitsInt(ref result, i, 22, cpv[j]);
                i += 22;
            }
            for (int j = 0; j < NCell; j++)
            {
                BitOperation.SetBitsUint(ref result, i, 4, plock[j]);
                i += 4;
            }
            for (int j = 0; j < NCell; j++)
            {
                BitOperation.SetBitsUint(ref result, i, 1, half[j]);
                i += 1;
            }
            for (int j = 0; j < NCell; j++)
            {
                BitOperation.SetBitsUint(ref result, i, 6, cnr[j]);
                i += 6;
            }
            EncodeRTCM3(ref result, bitLength - 48);
            return result;
        }

        public void RTD()
        {
            cpv.AsSpan().Fill(0);
            plock.AsSpan().Fill(0);
        }
    }
}
