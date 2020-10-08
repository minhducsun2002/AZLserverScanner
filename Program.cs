using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AZLServerProtobuf;
using AZLServerProtobuf.Commands;
using ProtoBuf;

namespace AZLserverScanner
{
    public class CheckResult
    {
        public enum CheckCode
        {
            Timeout,
            Disconnected,
            Success,
            Error
        }

        public CheckCode Code { get; set; }
        public string Comment { get; set; }
    }
    class Program
    {
        //<host>                            <start port>    <count> <timeout>   <threads count> <buff size in bytes>
        //audit.us.yo-star.com  [ip|url]    10              50      1500        20              1024
        static void Main(string[] args)
        {
            var host = args[0];
            var isUrl = args[1] == "url";
            var startPort = int.Parse(args[2]);
            var portCount = int.Parse(args[3]);
            var timeout = int.Parse(args[4]);
            var threadsCount = int.Parse(args[5]);
            var buffSize = int.Parse(args[6]);

            var ports = Enumerable.Range(startPort, portCount).ToArray();
            var stopwatch = new Stopwatch();

            if (!Directory.Exists("packets/"))
            {
                Directory.CreateDirectory("packets/");
            }
            
            stopwatch.Start();

            CheckServerPorts(host, ports, isUrl, timeout, threadsCount, buffSize);

            stopwatch.Stop();
            Console.WriteLine($"Total elapsed time: {stopwatch.Elapsed}");
        }


        static void CheckServerPorts(string host, int[] ports, bool isUrl, int timeout = 3000, int threads = 20, int buffSize = 1024)
        {
            var cOrig = Console.BackgroundColor;

            var hostIp = isUrl ? Dns.GetHostEntry(host).AddressList[0] : IPAddress.Parse(host);

            var packet = BuildCS10800();
            int i = 0;
            Parallel.ForEach(ports, new ParallelOptions() {MaxDegreeOfParallelism = threads}, (port) =>
            {
                var result = CheckServerPort(hostIp, port, timeout, packet, buffSize);
                if (result.Code == CheckResult.CheckCode.Success)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Checked ports: {i++}\tPort: {port}\tCode: {result.Code.ToString()}\n" + result.Comment);
                    Console.BackgroundColor = cOrig;
                    File.WriteAllText($"packets/{host}_{port}.txt", result.Comment);
                }
                else if (result.Code == CheckResult.CheckCode.Disconnected)
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine($"Checked ports: {i++}\tPort: {port}\tCode: {result.Code.ToString()}\n");
                    Console.BackgroundColor = cOrig;
                    File.AppendAllText($"packets/{host}_disconnected.txt", port.ToString()+"\n");
                }
                else if (result.Code == CheckResult.CheckCode.Error)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Checked ports: {i++}\tPort: {port}\tCode: {result.Code.ToString()}");
                    Console.BackgroundColor = cOrig;
                    File.AppendAllText($"packets/{host}_error.txt", port.ToString() + "\n");
                }
                else
                {
                    Console.WriteLine($"Checked ports: {i++}\tPort: {port}\tCode: {result.Code.ToString()}");
                }
               
            });
        }

        static CheckResult CheckServerPort(IPAddress hostIp, int port, int timeout, byte[] packet, int buffSize)
        {
            var buffer = new byte[buffSize];

            try
            {
                var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = timeout
                };
            
                IAsyncResult result = clientSocket.BeginConnect(hostIp, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(timeout, true);

                if (!success)
                {
                    return new CheckResult(){Code = CheckResult.CheckCode.Timeout};
                }

                clientSocket.Send(packet);

                if (clientSocket.Receive(buffer) == 0)
                {
                    return new CheckResult() { Code = CheckResult.CheckCode.Disconnected };
                }
                return new CheckResult() { Code = CheckResult.CheckCode.Success, Comment = ReadPacket(buffer) };
            }
            catch (Exception)
            {
                return new CheckResult() { Code = CheckResult.CheckCode.Error};
            }

            

        }

        static byte[] BuildCS10800(string channelId = "0" , uint state = 21)
        {
            MemoryStream memoryStream = new MemoryStream();
            Serializer.Serialize<CS10800>(memoryStream, new CS10800
            {
                State = state,
                Platform = channelId
            });
            byte[] array = memoryStream.ToArray();
            memoryStream.Close();
            PackStream packStream = new PackStream(7 + array.Length);
            packStream.WriteUint16((uint)(5 + array.Length));
            packStream.WriteUint8(0u);
            packStream.WriteUint16(10800u);
            packStream.WriteUint16(0u);
            packStream.WriteBuffer(array);
            return packStream.ToArray();
        }


        static string ReadPacket(byte[] buffer)
        {
            string result;
            int start = 0;
            int packetLength = buffer[start] << 8 | buffer[start + 1];
            int command = buffer[start + 3] << 8 | buffer[start + 4];
            int idx = buffer[start + 5] << 8 | buffer[start + 6];
            //byte[] protoBody = buffer.Skip(7).Take(packetLength-5).ToArray();
            result = $"command: {command} | idx: {idx}\n";
            switch (command)
            {
                case 10800:
                {
                    using (MemoryStream stream = new MemoryStream(buffer, start + 7, packetLength - 5))
                    {
                        var helloToServer = Serializer.Deserialize<CS10800>(stream);
                        result = result + $"State: {helloToServer.State} | Platform: {helloToServer.Platform}";
                    }
                    break;
                }
                case 10801:
                {
                    using (MemoryStream stream = new MemoryStream(buffer, start + 7, packetLength - 5))
                    {
                        var helloFromServer = Serializer.Deserialize<SC10801>(stream);
                        result = result + helloFromServer.ToString(); //$"GatewayIp: {helloFromServer.GatewayIp} | GatewayPort: {helloFromServer.GatewayPort}";
                    }
                    break;
                }
                default:
                {
                    if (packetLength > buffer.Length)
                    {
                        result = result +$"Unknown Packet \nProtobuf body:\nLength great then buffer: {packetLength} bytes";
                    }
                    else
                    {
                        result = result + "Unknown Packet \nProtobuf body:\n" + BitConverter.ToString(buffer, start + 7, packetLength - 7);
                    }
                    break;
                }
            }

            return result;

        }
    }
}
