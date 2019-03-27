using System;
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
            Success
        }

        public CheckCode Code { get; set; }
        public string Comment { get; set; }
    }
    class Program
    {
        
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            var ports = Enumerable.Range(int.Parse(args[1]), int.Parse(args[2])).ToArray(); //Enumerable.Range(1, 50).ToArray();
            var host = args[0]; //"audit.us.yo-star.com";
            stopwatch.Start();
            CheckServerPorts(host, ports, int.Parse(args[3]));
            stopwatch.Stop();
            Console.WriteLine($"Total elapsed time: {stopwatch.Elapsed}");
        }


        static void CheckServerPorts(string host, int[] ports, int timeout = 3000, int threads = 20)
        {
            var cOrig = Console.BackgroundColor;
            var hostIp = Dns.GetHostEntry(host).AddressList[0];
            var packet = BuildCS10800();
            int i = 0;
            Parallel.ForEach(ports, new ParallelOptions() {MaxDegreeOfParallelism = threads}, (port) =>
            {
                var result = CheckServerPort(hostIp, port, timeout, packet);
                var resultString = "";
                if (result.Comment != null)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Checked ports: {i++}\tPort: {port}\tCode: {result.Code.ToString()}\n" + result.Comment);
                    Console.BackgroundColor = cOrig;
                }
                else
                {
                    Console.WriteLine($"Checked ports: {i++}\tPort: {port}\tCode: {result.Code.ToString()}");
                }
               
            });
        }

        static CheckResult CheckServerPort(IPAddress hostIp, int port, int timeout, byte[] packet)
        {
            var buffer = new byte[20000];

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

            return new CheckResult() {Code = CheckResult.CheckCode.Success, Comment = ReadPacket(buffer)};

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
                        result = result + $"GatewayIp: {helloFromServer.GatewayIp} | GatewayPort: {helloFromServer.GatewayPort}";
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
