using System;

namespace RTCM3.RTCM3Message
{
    public class RTCM3_1230 : RTCM3Base
    {
        public uint MessageType;
        public uint StationID;
        public uint cpdSYNC;
        public uint Reserved;
        public uint FDAMMask;
        public int l1ca;
        public int l1p;
        public int l2ca;
        public int l2p;
        public RTCM3_1230(ReadOnlySpan<byte> databody)
        {
            int i = 0;
            int length;
            MessageType = BitOperation.GetBitsUint(databody, i, length = 12);
            i += length;
            StationID = BitOperation.GetBitsUint(databody, i, length = 12);
            i += length;
            cpdSYNC = BitOperation.GetBitsUint(databody, i, length = 1);
            i += length;
            Reserved = BitOperation.GetBitsUint(databody, i, length = 3);
            i += length;
            FDAMMask = BitOperation.GetBitsUint(databody, i, length = 4);
            i += length;
            if ((FDAMMask & 0b1000u) > 0)
            {
                l1ca = BitOperation.GetBitsInt(databody, i, length = 16);
                i += length;
            }
            if ((FDAMMask & 0b0100u) > 0)
            {
                l1p = BitOperation.GetBitsInt(databody, i, length = 16);
                i += length;
            }
            if ((FDAMMask & 0b0010u) > 0)
            {
                l2ca = BitOperation.GetBitsInt(databody, i, length = 16);
                i += length;
            }
            if ((FDAMMask & 0b0001u) > 0)
            {
                l2p = BitOperation.GetBitsInt(databody, i, 16);
            }
        }

        public override Memory<byte> Encode()
        {
            throw new NotImplementedException();
        }
    }
}
