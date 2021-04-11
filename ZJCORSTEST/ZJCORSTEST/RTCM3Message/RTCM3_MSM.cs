using System;
using System.Collections;

namespace RTCM3.RTCM3Message
{
    public class RTCM3_MSM : RTCM3Base
    {
        public uint MessageType;
        public uint StationID;
        public uint GNSStime;
        public uint Sync;
        public uint IODS;
        public uint Reserved;
        public uint ClockSync;
        public uint ClockExt;
        public uint Smooth;
        public uint TintS;
        public ulong GNSSSatMask;
        public uint GNSSSigMask;
        internal int i;
        public uint SatNumber;
        public uint SigNumber;
        public uint NCell;
        public BitArray Cell;

        public RTCM3_MSM(ReadOnlySpan<byte> databody)
        {
            int length;
            MessageType = BitOperation.GetBitsUint(databody, i, length = 12);
            i += length;
            StationID = BitOperation.GetBitsUint(databody, i, length = 12);
            i += length;
            GNSStime = BitOperation.GetBitsUint(databody, i, length = 30);
            i += length;
            Sync = BitOperation.GetBitsUint(databody, i, length = 1);
            i += length;
            IODS = BitOperation.GetBitsUint(databody, i, length = 3);
            i += length;
            Reserved = BitOperation.GetBitsUint(databody, i, length = 7);
            i += length;
            ClockSync = BitOperation.GetBitsUint(databody, i, length = 2);
            i += length;
            ClockExt = BitOperation.GetBitsUint(databody, i, length = 2);
            i += length;
            Smooth = BitOperation.GetBitsUint(databody, i, length = 1);
            i += length;
            TintS = BitOperation.GetBitsUint(databody, i, length = 3);
            i += length;
            GNSSSatMask = BitOperation.GetBitsUlong(databody, i, length = 64);
            i += length;
            GNSSSigMask = BitOperation.GetBitsUint(databody, i, length = 32);
            i += length;
            for (int j = 0; j < 64; j++)
            {
                ulong mask = ((1ul << j) & GNSSSatMask) >> j;
                SatNumber += (uint)mask;
            }
            for (int j = 0; j < 32; j++)
            {
                uint mask = ((1u << j) & GNSSSigMask) >> j;
                SigNumber += mask;
            }

            Cell = new BitArray((int)(SatNumber * SigNumber));
            for (int j = 0; j < Cell.Length; j++)
            {
                bool temp = BitOperation.GetBitsUint(databody, i, length = 1) > 0;
                if (temp)
                {
                    NCell++;
                    Cell.Set(j, temp);
                }
                i += length;
            }
        }
        public RTCM3_MSM()
        {

        }
        public void EncodeMSMHeader(ref Memory<byte> bytes)
        {
            int i = 24;
            int length;
            BitOperation.SetBitsUint(ref bytes, i, length = 12, MessageType);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 12, StationID);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 30, GNSStime);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 1, Sync);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 3, IODS);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 7, Reserved);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 2, ClockSync);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 2, ClockExt);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 1, Smooth);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 3, TintS);
            i += length;
            BitOperation.SetBitsUlong(ref bytes, i, length = 64, GNSSSatMask);
            i += length;
            BitOperation.SetBitsUint(ref bytes, i, length = 32, GNSSSigMask);
            i += length;

            for (int j = 0; j < Cell.Length; j++)
            {
                bool temp = Cell.Get(j);
                if (temp)
                {
                    BitOperation.SetBitsUint(ref bytes, i, length = 1, 1);
                }
                i += length;
            }
        }

        public override Memory<byte> Encode()
        {
            throw new NotImplementedException();
        }
    }
}
