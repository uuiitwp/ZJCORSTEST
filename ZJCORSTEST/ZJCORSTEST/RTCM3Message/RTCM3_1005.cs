using System;

namespace RTCM3.RTCM3Message
{
    public class RTCM3_1005 : RTCM3Base
    {
        public uint MessageType;
        public uint StationID;
        public uint ITRF;
        public uint GPS;
        public uint Glonass;
        public uint Galileo;
        public uint ReferenceStation;
        public double X;
        public uint OSC;
        public uint Beidou;
        public double Y;
        public uint QuartCycle;
        public double Z;

        public RTCM3_1005(ReadOnlySpan<byte> databody)
        {
            int i = 0;
            int length;
            MessageType = BitOperation.GetBitsUint(databody, i, length = 12);
            i += length;
            StationID = BitOperation.GetBitsUint(databody, i, length = 12);
            i += length;
            ITRF = BitOperation.GetBitsUint(databody, i, length = 6);
            i += length;
            GPS = BitOperation.GetBitsUint(databody, i, length = 1);
            i += length;
            Glonass = BitOperation.GetBitsUint(databody, i, length = 1);
            i += length;
            Galileo = BitOperation.GetBitsUint(databody, i, length = 1);
            i += length;
            ReferenceStation = BitOperation.GetBitsUint(databody, i, length = 1);
            i += length;
            X = BitOperation.GetBitsLong(databody, i, length = 38) * 0.0001;
            i += length;
            OSC = BitOperation.GetBitsUint(databody, i, length = 1);
            i += length;
            Beidou = BitOperation.GetBitsUint(databody, i, length = 1);
            i += length;
            Y = BitOperation.GetBitsLong(databody, i, length = 38) * 0.0001;
            i += length;
            QuartCycle = BitOperation.GetBitsUint(databody, i, length = 2);
            i += length;
            Z = BitOperation.GetBitsLong(databody, i, 38) * 0.0001;
        }
        public RTCM3_1005()
        {
            MessageType = 1005;
        }
        public override Memory<byte> Encode()
        {
            Memory<byte> result = new Memory<byte>(new byte[200 / 8]);
            int i = 24;
            int length;
            BitOperation.SetBitsUint(ref result, i, length = 12, MessageType);
            i += length;
            BitOperation.SetBitsUint(ref result, i, length = 12, StationID);
            i += length;
            BitOperation.SetBitsUint(ref result, i, length = 6, ITRF);
            i += length;
            BitOperation.SetBitsUint(ref result, i, length = 1, GPS);
            i += length;
            BitOperation.SetBitsUint(ref result, i, length = 1, Glonass);
            i += length;
            BitOperation.SetBitsUint(ref result, i, length = 1, Galileo);
            i += length;
            BitOperation.SetBitsUint(ref result, i, length = 1, ReferenceStation);
            i += length;
            BitOperation.SetBitsLong(ref result, i, length = 38, (long)(X / 0.0001 + (X > 0 ? 0.5 : -0.5)));
            i += length;
            BitOperation.SetBitsUint(ref result, i, length = 1, OSC);
            i += length;
            BitOperation.SetBitsUint(ref result, i, length = 1, Beidou);
            i += length;
            BitOperation.SetBitsLong(ref result, i, length = 38, (long)(Y / 0.0001 + (Y > 0 ? 0.5 : -0.5)));
            i += length;
            BitOperation.SetBitsUint(ref result, i, length = 2, QuartCycle);
            i += length;
            BitOperation.SetBitsLong(ref result, i, length = 38, (long)(Z / 0.0001 + (Z > 0 ? 0.5 : -0.5)));
            i += length;
            EncodeRTCM3(ref result, i - 24);
            return result;
        }

    }
}
