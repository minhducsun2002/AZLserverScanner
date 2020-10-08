using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

namespace AZLServerProtobuf.Commands
{
    [ProtoContract]
    class SC10801 : CommandBase
    {
        public static int CommandID = 10801;

        public SC10801()
        {
            Description = "Packet mean hello from server";
            IsCS = false;
        }

        public override string ToString()
        {
            return $"ID {CommandID}| GatewayIp {GatewayIp}, GatewayPort {GatewayPort}, Url {Url}, Versions {string.Join("|", Versions)}, ProxyIp {ProxyIp}, ProxyPort {ProxyPort}";
        }

        [ProtoMember(1, IsRequired = true, Name = "gateway_ip", DataFormat = DataFormat.Default)]
        public string GatewayIp { get; set; }

        [ProtoMember(2, IsRequired = true, Name = "gateway_port", DataFormat = DataFormat.TwosComplement)]
        public uint GatewayPort { get; set; }

        [ProtoMember(3, IsRequired = true, Name = "url", DataFormat = DataFormat.Default)]
        public string Url { get; set; }

        [ProtoMember(4, Name = "version", DataFormat = DataFormat.Default)]
        public List<string> Versions { get; set; }

        [ProtoMember(5, IsRequired = false, Name = "proxy_ip", DataFormat = DataFormat.Default)]
        [DefaultValue("")]
        public string ProxyIp { get; set; }

        [ProtoMember(6, IsRequired = false, Name = "proxy_port", DataFormat = DataFormat.TwosComplement)]
        [DefaultValue(0L)]
        public uint ProxyPort { get; set; }
    }
}
