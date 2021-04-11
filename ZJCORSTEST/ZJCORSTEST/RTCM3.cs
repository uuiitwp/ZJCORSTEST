using RTCM3.RTCM3Message;
using System;
namespace RTCM3
{
    public class RTCM3
    {
        private const uint Preamble = 0xd3;
        private const uint Reserved = 0;
        public readonly uint DataBytesLength;
        public readonly uint MessageType;
        public readonly RTCM3Base Databody;
        public RTCM3(ReadOnlySpan<byte> bytes, bool checkCRC24Q = true, bool decode = true)
        {
            if (bytes.Length < 6)
            {
                throw new InvalidCastException($"invalid Length:{bytes.Length}");
            }
            uint preamble = BitOperation.GetBitsUint(bytes, 0, 8);
            if (preamble != Preamble)
            {
                throw new InvalidCastException($"invalid preamble:{preamble:X2}");
            }
            uint reserved = BitOperation.GetBitsUint(bytes, 8, 6);
            if (reserved != Reserved)
            {
                throw new InvalidCastException($"invalid reserved:{reserved:X2}");
            }
            DataBytesLength = BitOperation.GetBitsUint(bytes, 14, 10);
            if (bytes.Length < DataBytesLength + 6)
            {
                throw new InvalidCastException($"invalid data bytes length:{DataBytesLength}");
            }
            if (checkCRC24Q)
            {
                if (!ValidCRC24Q(bytes))
                {
                    throw new InvalidCastException("invalid CRC24Q");
                }
            }
            MessageType = BitOperation.GetBitsUint(bytes, 24, 12);
            if (decode)
            {
                ReadOnlySpan<byte> body = bytes[3..(int)(3 + DataBytesLength)];
                Databody = MessageType switch
                {
                    1005 => new RTCM3_1005(body),
                    1007 => new RTCM3_1007(body),
                    1032 => new RTCM3_1032(body),
                    1033 => new RTCM3_1033(body),
                    1074 => new RTCM3_MSM4(body),
                    1084 => new RTCM3_MSM4(body),
                    1124 => new RTCM3_MSM4(body),
                    1230 => new RTCM3_1230(body),
                    _ => default,
                };
            }
        }
        private bool ValidCRC24Q(ReadOnlySpan<byte> bytes)
        {
            int crcposition = (int)(DataBytesLength + 3);
            return CRC24Q.Get(bytes[..crcposition]) == BitOperation.GetBitsUint(bytes[crcposition..], 0, 24);
        }

        public override string ToString()
        {
            string s = string.Empty;
            switch(MessageType)
            {
                case 1004:
                    s = $"GPS观测值";
                    break;
                case 1012:
                    s = $"GLONASS观测值";
                    break;
                case 1005:
                    var m1005 = Databody as RTCM3_1005;
                    s = $"参考站坐标:\r\nX:{m1005.X}\r\nY:{m1005.Y}\r\nZ:{m1005.Z}";
                    break;
                case 1007:
                    var m1007 = Databody as RTCM3_1007;
                    s = $"天线信息:\r\n{m1007.AntDescriptor}";
                    break;
                case 1019:
                    s = $"GPS星历";
                    break;
                case 1020:
                    s = $"GLONASS星历";
                    break;
                case 1030:
                    s = $"GPS网络RTK残差电文";
                    break;
                case 1031:
                    s = $"GLONASS网络RTK残差电文";
                    break;
                case 1032:
                    var m1032 = Databody as RTCM3_1032;
                    s = $"物理参考站坐标:\r\nX:{m1032.ReferenceStationX}\r\nY:{m1032.ReferenceStationY}\r\nZ:{m1032.ReferenceStationZ}";
                    break;
                case 1033:
                    var m1033 = Databody as RTCM3_1033;
                    s = $"天线:{m1033.AntDescriptor}\r\n接收机:{m1033.ReceiverDescriptor}";
                    break;
                case 1230:
                    s = $"GLONASS偏差信息";
                    break;
                case 1074:
                    var m1074 = Databody as RTCM3_MSM4;
                    s = $"GPS观测值，卫星个数{m1074.SatNumber}";
                    break;
                case 1084:
                    var m1084 = Databody as RTCM3_MSM4;
                    s = $"GLONASS观测值，卫星个数{m1084.SatNumber}";
                    break;
                case 1124:
                    var m1124 = Databody as RTCM3_MSM4;
                    s = $"BEIDOU观测值，卫星个数{m1124.SatNumber}";
                    break;
                default:
                    s = $"不支持解析的类型{MessageType}";
                    break;
            }
            return s;
        }
    }
}
