using NmeaParser.Messages;
using Ntrip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ZJCORSTEST
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        IPAddress _IP;
        int _PORT;

        async Task<bool> Check()
        {
            var result = true;
            if (!IPAddress.TryParse(IP.Text, out _IP))
            {
                await AddMessage($"{IP.Text}不是一个有效的IP地址");
                result = false;
            }
            if (!int.TryParse(PORT.Text, out _PORT) || 0 > _PORT || _PORT > 65535)
            {
                await AddMessage($"{PORT.Text}不是一个有效的端口");
                result = false;
            }
            if (string.IsNullOrEmpty(USER.Text))
            {
                await AddMessage($"用户名为空");
                result = false;
            }
            if (string.IsNullOrEmpty(PWD.Text))
            {
                await AddMessage($"密码为空");
                result = false;
            }
            return result;
        }
        private static string D2DM_Abs(double d, bool isLatitude)
        {
            d = Math.Abs(d);
            int dd = (int)d;
            double m = Math.Abs((d - dd) * 60);
            return (isLatitude ? dd.ToString("00") : dd.ToString("000")) + m.ToString("00.0000");
        }
        Client client;
        private async void Button_Clicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if(btn.Text == "运行")
            {
                btn.Text = "停止";
            }
            else
            {
                client?.Close();
                btn.Text = "运行";
                return;
            }
            try
            {
                if (!await Check())
                {
                    await AddMessage($"由于以上错误，无法运行");
                    return;
                }
                if (MP.SelectedIndex < 0)
                {
                    await AddMessage($"未选择源节点");
                    return;
                }
                double _B;
                double _L;
                double _H;
                if (!double.TryParse(B.Text, out _B) || _B < -90 || _B > 90)
                {
                    await AddMessage($"纬度数据无效");
                    return;
                }
                if (!double.TryParse(L.Text, out _L) || _L < -180 || _L > 180)
                {
                    await AddMessage($"经度数据无效");
                    return;
                }
                if (!double.TryParse(H.Text, out _H))
                {
                    await AddMessage($"大地高数据无效");
                    return;
                }
                client = new Client(5000, 5000);
                await client.ConnectAsync(_IP.ToString(), _PORT);
                await AddMessage($"连接成功");
                string authorization = $"Authorization: Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{USER.Text}:{PWD.Text}"))}\r\n";
                var Request = Encoding.UTF8.GetBytes(
                        $"GET /{MP.SelectedItem} HTTP/1.0\r\n" +
                        "User-Agent: uuiitwp\r\n" +
                        authorization +
                        "Accept: */*\r\n\r\n");
                await client.SendAsync(Request);
                _ = Task.Run(async () =>
                {
                    for (; ; )
                    {
                        await Task.Delay(2000);
                        Gga gga = new Gga("GPGGA", new string[]{
                                DateTime.UtcNow.ToString("HHmmss.ff"),
                                D2DM_Abs(_B,true),
                                _B>0? "N":"S"  ,
                                D2DM_Abs(_L,false),
                                _L>0?"E":"W"  ,
                                "4",
                                "12",
                                "1.0",
                                _H.ToString("0.000"),
                                "M",
                                "0.000",
                                "M",
                                "0.0",
                                ""});
                        await client.SendAsync(Encoding.UTF8.GetBytes(gga.ToString() + "\r\n"));
                        await Task.Delay(3000);
                    }
                });
                client.Start();
                for (; ; )
                {
                    var rtcm3 = await client.ReceiveAsync();
                    await AddMessage($"接收到基准站信号{rtcm3.MessageType}:\r\n{rtcm3}");

                }
            }
            catch (Exception ex)
            {
                await AddMessage($"{ex.Message}");
            }
            finally
            {
                client?.Close();
                btn.Text = "运行";
            }
        }

        async Task AddMessage(string s)
        {
            var label = new Label();
            label.Text = DateTime.Now.ToString("HH:mm:ss") + "  " + s;
            label.Margin = new Thickness(10, 0, 10, 0);
            if (StackLayout1.Children.Count > 100)
            {
                StackLayout1.Children.Remove(StackLayout1.Children.First());
            }
            StackLayout1.Children.Add(label);
            await ScrollView1.ScrollToAsync(label, ScrollToPosition.MakeVisible, false);
        }

        private async Task getmp()
        {
            TcpClient client = new TcpClient();
            try
            {

                client.ReceiveTimeout = 5000;
                client.SendTimeout = 5000;
                await client.ConnectAsync(_IP, _PORT);
                var s = client.GetStream();
                s.ReadTimeout = 5000;
                s.WriteTimeout = 5000;
                var r = Encoding.UTF8.GetBytes("GET / HTTP/1.0\r\nUser-Agent: uuiitwp\r\nAccept: */*\r\n\r\n");
                s.Write(r, 0, r.Length);
                var read = new byte[4096];
                var len = s.Read(read, 0, 4096);
                var sr = Encoding.UTF8.GetString(read[..len]);
                var t = sr.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (t.Length != 2)
                {
                    throw new Exception("服务器返回数据格式错误");
                }
                var sts = t[1].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (sts.Length < 2)
                {
                    throw new Exception("服务器返回数据格式错误");
                }
                MP.Items.Clear();
                for (int i = 0; i < sts.Length - 1; i++)
                {
                    var items = sts[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    MP.Items.Add(items[1]);
                }
                MP.SelectedIndex = 0;
                await AddMessage($"获取源节点成功");
            }
            finally
            {
                client.Dispose();
            }
        }
        private async void Button_Clicked_1(object sender, EventArgs e)
        {

            try
            {
                ((Button)sender).IsEnabled = false;
                if (!await Check())
                {
                    throw new Exception("信息输入错误");
                }
                var t = getmp();
                if (await Task.WhenAny(t, Task.Delay(5000)) != t)
                {
                    throw new Exception("响应超时");
                }
            }
            catch (Exception ex)
            {
                await AddMessage(ex.Message);
                await AddMessage($"由于以上错误,无法获取挂载点");
                return;
            }
            finally
            {
                ((Button)sender).IsEnabled = true;
            }
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            StackLayout1.Children.Clear();
        }
    }
}
