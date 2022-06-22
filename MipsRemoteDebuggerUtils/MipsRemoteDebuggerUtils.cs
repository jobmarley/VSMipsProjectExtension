using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MipsRemoteDebuggerUtils
{

    public enum md_status
        : uint
    {
        Success = 0,
        InvalidArg = 1,
        Failure = 2,
    }
    public enum md_event
        : uint
    {
        Breakpoint = 0,
    }
    public enum md_register
        : uint
    {
        md_register_pc = 0,
        md_register_at = 1,
        md_register_v0 = 2,
        md_register_v1 = 3,
        md_register_a0 = 4,
        md_register_a1 = 5,
        md_register_a2 = 6,
        md_register_a3 = 7,
        md_register_t0 = 8,
        md_register_t1 = 9,
        md_register_t2 = 10,
        md_register_t3 = 11,
        md_register_t4 = 12,
        md_register_t5 = 13,
        md_register_t6 = 14,
        md_register_t7 = 15,
        md_register_s0 = 16,
        md_register_s1 = 17,
        md_register_s2 = 18,
        md_register_s3 = 19,
        md_register_s4 = 20,
        md_register_s5 = 21,
        md_register_s6 = 22,
        md_register_s7 = 23,
        md_register_t8 = 24,
        md_register_t9 = 25,
        md_register_k0 = 26,
        md_register_k1 = 27,
        md_register_gp = 28,
        md_register_sp = 29,
        md_register_fp = 30,
        md_register_ra = 31,
    }

    public enum md_state
        : uint
    {
        md_state_enabled = 0x1,
    }

    public enum PacketType
        : uint
    {
        ReadMemoryRequest,
        ReadMemoryResponse,
        WriteMemoryRequest,
        WriteMemoryResponse,
        ReadRegisterRequest,
        ReadRegisterResponse,
        WriteRegisterRequest,
        WriteRegisterResponse,
        ReadStateRequest,
        ReadStateResponse,
        WriteStateRequest,
        WriteStateResponse,
    }
    class BinaryDeserializer
	{
        BinaryReader m_reader = null;
        public BinaryDeserializer(BinaryReader reader)
		{
            m_reader = reader;
        }
        public object Deserialize(Type t)
		{
            if (t == typeof(string))
                return m_reader.ReadString();
            if (t == typeof(byte[]))
			{
                uint count = m_reader.ReadUInt32();
                return m_reader.ReadBytes((int)count);
            }
            if (t == typeof(char))
                return m_reader.ReadChar();
            if (t == typeof(byte))
                return m_reader.ReadByte();
            if (t == typeof(short))
                return m_reader.ReadInt16();
            if (t == typeof(ushort))
                return m_reader.ReadUInt16();
            if (t == typeof(int))
                return m_reader.ReadInt32();
            if (t == typeof(uint))
                return m_reader.ReadUInt32();
            if (t == typeof(long))
                return m_reader.ReadInt64();
            if (t == typeof(ulong))
                return m_reader.ReadUInt64();
            if (t.IsEnum)
                return Enum.ToObject(t, Deserialize(t.GetEnumUnderlyingType()));

            throw new Exception("Unknown type");
        }
    }
    class BinarySerializer
    {
        BinaryWriter m_writer = null;
        public BinarySerializer(BinaryWriter writer)
        {
            m_writer = writer;
        }
        public void Serialize(object o)
        {
            if (o is string s)
                m_writer.Write(s);
            if (o is byte[] ba)
            {
                m_writer.Write((uint)ba.Length);
                m_writer.Write(ba);
            }
            if (o is char c)
                m_writer.Write(c);
            if (o is byte b)
                m_writer.Write(b);
            if (o is short sh)
                m_writer.Write(sh);
            if (o is ushort ush)
                m_writer.Write(ush);
            if (o is int i)
                m_writer.Write(i);
            if (o is uint ui)
                m_writer.Write(ui);
            if (o is long l)
                m_writer.Write(l);
            if (o is ulong ul)
                m_writer.Write(ul);
            if (o.GetType().IsEnum)
                Serialize(Convert.ChangeType(o, o.GetType().GetEnumUnderlyingType()));
                    
            throw new Exception("Unknown type");
        }
    }
    
    public class Packet
    {
        // IDs generated on server have 0x80000000 to avoid collisions
        public uint ID { get; set; }
        protected Packet()
        {
        }

        public static T Deserialize<T>(BinaryReader reader)
            where T : Packet, new()
        {
            T t = new T();
            BinaryDeserializer deserializer = new BinaryDeserializer(reader);
            foreach (System.Reflection.PropertyInfo pi in typeof(T).GetProperties())
            {
                object o = deserializer.Deserialize(pi.PropertyType);
                pi.SetValue(t, o);
            }
            return t;
        }
        public static byte[] Serialize(Packet p)
        {
            MemoryStream ms = new MemoryStream(100);
            BinaryWriter bw = new BinaryWriter(ms);
            BinarySerializer serializer = new BinarySerializer(bw);
            foreach (System.Reflection.PropertyInfo pi in p.GetType().GetProperties())
            {
                serializer.Serialize(pi.GetValue(p));
            }
            return ms.ToArray();
        }
    }
    public class PacketHeader
    {
        public PacketType Type { get; set; }
        public uint Length { get; set; }
    }
    public class ReadMemoryRequestPacket
        : Packet
    {
        public uint Count { get; set; }
        public uint Offset { get; set; }
    }
    public class ReadMemoryResponsePacket
        : Packet
    {
        public byte[] Data { get; set; }
        public md_status Status { get; set; }
    }
	public class WriteMemoryRequestPacket
		: Packet
	{
		public byte[] Data { get; set; }
		public uint Offset { get; set; }
	}
	public class WriteMemoryResponsePacket
		: Packet
	{
		public uint WrittenCount { get; set; }
        public md_status Status { get; set; }
    }
    public class ReadRegisterRequestPacket
        : Packet
    {
        public md_register Register { get; set; }
    }
    public class ReadRegisterResponsePacket
        : Packet
    {
        public md_register Register { get; set; }
        public uint Value { get; set; }
        public md_status Status { get; set; }
    }
    public class WriteRegisterRequestPacket
        : Packet
    {
        public md_register Register { get; set; }
        public uint Value { get; set; }
    }
    public class WriteRegisterResponsePacket
        : Packet
    {
        public md_status Status { get; set; }
    }
    public class ReadStateRequestPacket
        : Packet
    {
        public uint Value { get; set; }
    }
    public class ReadStateResponsePacket
        : Packet
    {
        public md_state State { get; set; }
        public md_status Status { get; set; }
    }
    public class WriteStateRequestPacket
        : Packet
    {
        public md_state State { get; set; }
    }
    public class WriteStateResponsePacket
        : Packet
    {
        public md_status Status { get; set; }
    }
    public class EventNotificationPacket
        : Packet
    {
        public md_event Event { get; set; }
    }
}
