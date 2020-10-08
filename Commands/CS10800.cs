using ProtoBuf;

namespace AZLServerProtobuf.Commands
{
    [ProtoContract]
    class CS10800 : CommandBase
    {
        public static int CommandID = 10800;

        public CS10800()
        {
            Description = "Packet mean hello to server";
            IsCS = true;
        }

        public override string ToString()
        {
            return $"ID {CommandID}| State {State}, Platform {Platform}";
        }

        [ProtoMember(1, IsRequired = true, Name = "state", DataFormat = DataFormat.TwosComplement)]
        public uint State { get; set; }

        [ProtoMember(2, IsRequired = true, Name = "platform", DataFormat = DataFormat.Default)]
        public string Platform { get; set; }
    }
}
