using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using MipsRemoteDebuggerUtils;

namespace MipsRemoteDebugger
{


    internal class Server
    {
        public Server()
        {


        }

        const int port = 79532;

        IntPtr m_devicePtr = IntPtr.Zero;

        async Task HandlePacketAsync(TcpClient client, Packet packet)
		{
            if (packet is ReadMemoryRequestPacket rmrp)
			{
                byte[] buffer = new byte[rmrp.Count];
                uint readCount = 0;
                md_status status = await Task.Run(() => mipsdebug_api.md_read_memory(m_devicePtr, buffer, (uint)buffer.Length, rmrp.Offset, out readCount));
                ReadMemoryResponsePacket resp = new ReadMemoryResponsePacket() { ID = rmrp.ID, Data = buffer, Status = status };
                await SendPacketAsync(client, resp);
            }
            else if (packet is WriteMemoryRequestPacket wmrp)
            {
                uint writtenCount = 0;
                md_status status = await Task.Run(() => mipsdebug_api.md_write_memory(m_devicePtr, wmrp.Data, (uint)wmrp.Data.Length, wmrp.Offset, out writtenCount));
                WriteMemoryResponsePacket resp = new WriteMemoryResponsePacket() { ID = wmrp.ID, WrittenCount = writtenCount, Status = status };
                await SendPacketAsync(client, resp);
            }
            else if (packet is ReadRegisterRequestPacket rrrp)
            {
                uint value = 0;
                md_status status = await Task.Run(() => mipsdebug_api.md_read_register(m_devicePtr, rrrp.Register, out value));
                ReadRegisterResponsePacket resp = new ReadRegisterResponsePacket() { ID = rrrp.ID, Register = rrrp.Register, Value = value, Status = status };
                await SendPacketAsync(client, resp);
            }
            else if (packet is WriteRegisterRequestPacket wrrp)
			{
                uint value = 0;
                md_status status = await Task.Run(() => mipsdebug_api.md_write_register(m_devicePtr, wrrp.Register, value));
                WriteRegisterResponsePacket resp = new WriteRegisterResponsePacket() { ID = wrrp.ID, Status = status };
                await SendPacketAsync(client, resp);
            }
            else if (packet is ReadStateRequestPacket rsrp)
            {
                md_state value = 0;
                md_status status = await Task.Run(() => mipsdebug_api.md_get_state(m_devicePtr, out value));
                ReadStateResponsePacket resp = new ReadStateResponsePacket() { ID = rsrp.ID, State = value, Status = status };
                await SendPacketAsync(client, resp);
            }
            else if (packet is WriteStateRequestPacket wsrp)
            {
                md_status status = await Task.Run(() => mipsdebug_api.md_set_state(m_devicePtr, wsrp.State));
                WriteStateResponsePacket resp = new WriteStateResponsePacket() { ID = wsrp.ID, Status = status };
                await SendPacketAsync(client, resp);
            }
            else
                throw new Exception("Unknown packet type");
		}
        async Task<Packet> ReceivePacketAsync(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            BinaryReader reader = new BinaryReader(ns);

            uint length = reader.ReadUInt32();
            if (length > 50 * 1024) // hard limit on 50KB
                throw new Exception("packet too big");

            byte[] buffer = reader.ReadBytes((int)length);
            BinaryReader bufferReader = new BinaryReader(new MemoryStream(buffer));
            PacketType type = (PacketType)bufferReader.ReadUInt32();

            switch (type)
            {
                case PacketType.ReadMemoryRequest:
                    return Packet.Deserialize<ReadMemoryRequestPacket>(bufferReader);
                case PacketType.ReadRegisterRequest:
                    return Packet.Deserialize<ReadRegisterRequestPacket>(bufferReader);
                case PacketType.WriteMemoryRequest:
                    return Packet.Deserialize<WriteMemoryRequestPacket>(bufferReader);
                case PacketType.WriteRegisterRequest:
                    return Packet.Deserialize<WriteRegisterRequestPacket>(bufferReader);
                case PacketType.ReadStateRequest:
                    return Packet.Deserialize<ReadStateRequestPacket>(bufferReader);
                case PacketType.WriteStateRequest:
                    return Packet.Deserialize<WriteStateRequestPacket>(bufferReader);
                default:
                    throw new Exception("Unknown packet type");
            }
        }

        async Task SendPacketAsync(TcpClient client, Packet packet)
		{
            await Task.Run(() => SendPacket(client, packet));
		}
        void SendPacket(TcpClient client, Packet packet)
        {
            byte[] buffer = Packet.Serialize(packet);
            PacketHeader hdr = new PacketHeader() { Length = (uint)buffer.Length + 8, Type =  };
            lock (client)
            {
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
        }
        int m_id = 0;
        uint GeneratePacketID()
		{
            int v = System.Threading.Interlocked.Increment(ref m_id);
            return 0x80000000 | (uint)v;
        }
        void OnMipsEvent(md_event e)
		{
            TcpClient client = Client;
            if (client == null)
                return;

            EventNotificationPacket packet = new EventNotificationPacket() { ID = GeneratePacketID(), Event = e };
            SendPacket(client, packet);
		}
        public TcpClient Client { get; private set; }
        async Task LoopAsync(TcpListener listener)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine(string.Format("Incoming connection from {0}", client.Client.RemoteEndPoint.ToString()));

            Client = client;

            while (true)
            {
				try
                {
                    Packet p = await ReceivePacketAsync(client);
                    await HandlePacketAsync(client, p);
                }
				catch (Exception e)
				{
                    Console.WriteLine("Exception: " + e.Message);
                    Console.WriteLine("Closing connection from " + client.Client.RemoteEndPoint.ToString());
                    break;
				}
            }
            Client = null;
        }
        public void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            Console.WriteLine(string.Format("Starting to listen on port {0}...", port));
            listener.Start();

            m_devicePtr = mipsdebug_api.md_open();
            mipsdebug_api.md_register_callback(m_devicePtr, OnMipsEvent);

            Task loopTask = Task.Run(async () =>
            {
                while (true)
                {
                    await LoopAsync(listener);
                }
            });

            while (true)
            {
                string s = Console.ReadLine();
                if (s == "exit")
                    break;
            }

            mipsdebug_api.md_unregister_callback(m_devicePtr, OnMipsEvent);
            mipsdebug_api.md_close(m_devicePtr);
        }
    }
}
