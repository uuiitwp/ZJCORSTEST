using System;
using System.Text;

namespace RTCM3.RTCM3Message
{
    public class RTCM3_1007 : RTCM3Base
    {
        public uint MessageType;
        public uint StationID;
        public uint AntDescriptorCounter;
        public string AntDescriptor;
        public uint AntSetupID;

        public RTCM3_1007(ReadOnlySpan<byte> databody)
        {
            int i = 0;
            int length;
            MessageType = BitOperation.GetBitsUint(databody, i, length = 12);
            i += length;
            StationID = BitOperation.GetBitsUint(databody, i, length = 12);
            i += length;
            AntDescriptorCounter = BitOperation.GetBitsUint(databody, i, length = 8);
            i += length;
            int antDescriptorPosition = i / 8;
            AntDescriptor = Encoding.ASCII.GetString(databody[antDescriptorPosition..(int)(antDescriptorPosition + AntDescriptorCounter)]);
            i += (int)AntDescriptorCounter * 8;
            AntSetupID = BitOperation.GetBitsUint(databody, i, 8);
        }

        public override Memory<byte> Encode()
        {
            throw new NotImplementedException();
        }
    }
}
