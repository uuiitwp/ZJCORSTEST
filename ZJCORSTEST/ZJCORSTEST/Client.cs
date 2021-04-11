using RTCM3;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ntrip
{
    public class Client
    {
        public TcpClient client;
        public Pipe pipe;
        private readonly int bufferSize;
        private readonly int SendTimeout;
        private readonly int ReceiveTimeout;
        public bool Connected => client?.Connected ?? false;


        private NetworkStream Stream;
        public Client(int SendTimeout, int ReceiveTimeout, int bufferSize = 4096)
        {
            this.bufferSize = bufferSize;
            this.SendTimeout = SendTimeout;
            this.ReceiveTimeout = ReceiveTimeout;


        }
        public async Task ConnectAsync(string host, int port)
        {
            client = new TcpClient
            {
                SendTimeout = SendTimeout,
                ReceiveTimeout = ReceiveTimeout
            };
            await client.ConnectAsync(host, port);
            Stream = client.GetStream();
        }

        public async Task ConnectAsync(IPEndPoint iPEndPoint)
        {
            await ConnectAsync(iPEndPoint.Address.ToString(), iPEndPoint.Port);
        }

        public void Start()
        {
            pipe = new Pipe();
            Task.Run(async () => { await Run(); });

        }
        Memory<byte> Lastbuffer;
        public async Task Run()
        {
            Memory<byte> buffer = new Memory<byte>(new byte[bufferSize]);
            try
            {
                for (; ; )
                {
                    if (Connected)
                    {
                        int len = await Stream.ReadAsync(buffer);
                        if (len == 0)
                        {
                            break;
                        }
                        Lastbuffer = buffer[..len];
                        await pipe.Writer.WriteAsync(Lastbuffer);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                pipe.Writer.Complete();
                //pipe.Reader.Complete();
                //client.Dispose();
            }
        }

        public async Task SendAsync(ReadOnlyMemory<byte> buffer)
        {
            await Stream.WriteAsync(buffer);
        }

        public async Task<RTCM3.RTCM3> ReceiveAsync()
        {
            if (pipe is null || client.Connected == false)
            {
                throw new InvalidOperationException();
            }
            else
            {
                PipeReader reader = pipe.Reader;

                RTCM3.RTCM3 result;
                do
                {
                    ReadResult readresult = await reader.ReadAsync();
                    ReadOnlySequence<byte> buffer = readresult.Buffer;
                    if (buffer.Length == 0)
                    {
                        if (success)
                        {
                            throw new Exception("连接被关闭");
                        }
                        if (Lastbuffer.Length != 0)
                        {
                            string t;
                            var array = Lastbuffer.ToArray();
                            string ser;
                            try
                            {
                                t = Encoding.UTF8.GetString(array);
                                if (t.Contains("username", StringComparison.OrdinalIgnoreCase) || t.Contains("password", StringComparison.OrdinalIgnoreCase) || t.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
                                {
                                    t += "\r\n分析以上数据可能是用户名密码错误";
                                }
                                ser = "连接被关闭,服务器最后返回的数据为:\r\n" + t;
                            }
                            catch
                            {
                                try
                                {
                                    t = Encoding.ASCII.GetString(array);
                                    if (t.Contains("username", StringComparison.OrdinalIgnoreCase) || t.Contains("password", StringComparison.OrdinalIgnoreCase) || t.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
                                    {
                                        t += "\r\n分析以上数据可能是用户名密码错误";
                                    }
                                    ser = "连接被关闭,服务器最后返回的数据为:\r\n" + t;
                                }
                                catch
                                {
                                    var returnStr = string.Empty;
                                    for (int i = 0; i < array.Length; i++)
                                    {
                                        returnStr += array[i].ToString("X2") + " ";
                                    }
                                    ser = "连接被关闭,服务器最后返回的数据为:\r\n" + returnStr;
                                }
                            }
                            throw new Exception(ser);
                        }
                        throw new Exception("连接被关闭,服务器未返回任何数据");
                    }
                    result = Filter(ref buffer);
                    reader.AdvanceTo(buffer.Start);
                } while (result is null);
                return result;

            }
        }

        public void Close()
        {

            client?.Close();
            client?.Dispose();


        }
        bool success = false;
        private RTCM3.RTCM3 Filter(ref ReadOnlySequence<byte> buffer)
        {
            RTCM3.RTCM3 result = null;
            SequenceReader<byte> reader = new SequenceReader<byte>(buffer);
            if (reader.TryAdvanceTo(0xd3, false))
            {
                Span<byte> h1 = new Span<byte>(new byte[3]);
                if (!reader.TryCopyTo(h1))
                {
                    return null;
                }
                if (BitOperation.GetBitsUint(h1, 8, 6) == 0)
                {
                    uint len = BitOperation.GetBitsUint(h1, 14, 10) + 6;
                    h1 = new Span<byte>(new byte[len]);
                    if (!reader.TryCopyTo(h1))
                    {
                        return null;
                    }
                    try
                    {
                        result = new RTCM3.RTCM3(h1);
                        success = true;
                    }
                    catch
                    {
                        result = null;
                    }
                    buffer = buffer.Slice(len);
                }
                else
                {
                    buffer = buffer.Slice(3);
                }
            }
            else
            {
                buffer = buffer.Slice(buffer.Length);
            }
            return result;
        }
    }
}
