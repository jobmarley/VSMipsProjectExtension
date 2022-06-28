using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

using MipsRemoteDebuggerUtils;

namespace MipsRemoteDebugger
{
	internal class Connection
	{
		IntPtr m_devicePtr = IntPtr.Zero;
		object m_sendLock = new object();
		TcpClient m_client = null;

		public TcpClient Client => m_client;
		public Connection(IntPtr devicePtr, TcpClient client)
		{
			m_devicePtr = devicePtr;
			m_client = client;
		}

		public async Task LoopAsync(CancellationToken cancellationToken)
		{
			// This is async but single threaded
			// We just service packets in order, except when we send an event asynchronously
			try
			{
				while (true)
				{
					Packet p = await ReceivePacketAsync(cancellationToken);
					await HandlePacketAsync(p, cancellationToken);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception: " + e.Message);
			}
		}
		async Task HandlePacketAsync(Packet packet, CancellationToken cancellationToken)
		{
			if (packet is ReadMemoryRequestPacket rmrp)
			{
				byte[] buffer = new byte[rmrp.Count];
				uint readCount = 0;
				md_status status = await Task.Run(() => mipsdebug_api.md_read_memory(m_devicePtr, buffer, (uint)buffer.Length, rmrp.Offset, out readCount));
				ReadMemoryResponsePacket resp = new ReadMemoryResponsePacket() { ID = rmrp.ID, Data = buffer, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is WriteMemoryRequestPacket wmrp)
			{
				uint writtenCount = 0;
				md_status status = await Task.Run(() => mipsdebug_api.md_write_memory(m_devicePtr, wmrp.Data, (uint)wmrp.Data.Length, wmrp.Offset, out writtenCount));
				WriteMemoryResponsePacket resp = new WriteMemoryResponsePacket() { ID = wmrp.ID, WrittenCount = writtenCount, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is ReadRegisterRequestPacket rrrp)
			{
				uint value = 0;
				md_status status = await Task.Run(() => mipsdebug_api.md_read_register(m_devicePtr, rrrp.Register, out value));
				ReadRegisterResponsePacket resp = new ReadRegisterResponsePacket() { ID = rrrp.ID, Register = rrrp.Register, Value = value, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is WriteRegisterRequestPacket wrrp)
			{
				uint value = 0;
				md_status status = await Task.Run(() => mipsdebug_api.md_write_register(m_devicePtr, wrrp.Register, value));
				WriteRegisterResponsePacket resp = new WriteRegisterResponsePacket() { ID = wrrp.ID, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is ReadStateRequestPacket rsrp)
			{
				md_state value = 0;
				md_status status = await Task.Run(() => mipsdebug_api.md_get_state(m_devicePtr, out value));
				ReadStateResponsePacket resp = new ReadStateResponsePacket() { ID = rsrp.ID, State = value, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is WriteStateRequestPacket wsrp)
			{
				md_status status = await Task.Run(() => mipsdebug_api.md_set_state(m_devicePtr, wsrp.State));
				WriteStateResponsePacket resp = new WriteStateResponsePacket() { ID = wsrp.ID, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else
				throw new Exception("Unknown packet type");
		}

		// Doesnt need to be threadsafe since its only called from LoopAsync
		async Task<Packet> ReceivePacketAsync(CancellationToken cancellationToken)
		{
			NetworkStream ns = m_client.GetStream();
			BinaryReader reader = new BinaryReader(ns);

			byte[] hdrbuffer = new byte[8];
			if (await ns.ReadAsync(hdrbuffer, 0, hdrbuffer.Length, cancellationToken) != hdrbuffer.Length)
				throw new Exception();

			PacketHeader hdr = Packet.Deserialize<PacketHeader>(new BinaryReader(new MemoryStream(hdrbuffer)));
			if (hdr.Length > 50 * 1024) // hard limit on 50KB
				throw new Exception("packet too big");

			byte[] buffer = new byte[hdr.Length - 8];
			if (await ns.ReadAsync(buffer, 0, buffer.Length, cancellationToken) != buffer.Length)
				throw new Exception();

			BinaryReader bufferReader = new BinaryReader(new MemoryStream(buffer));
			switch (hdr.Type)
			{
				case PacketType.ReadMemoryRequest:
					return await Task.Run(() => Packet.Deserialize<ReadMemoryRequestPacket>(bufferReader), cancellationToken);
				case PacketType.ReadRegisterRequest:
					return await Task.Run(() => Packet.Deserialize<ReadRegisterRequestPacket>(bufferReader), cancellationToken);
				case PacketType.WriteMemoryRequest:
					return await Task.Run(() => Packet.Deserialize<WriteMemoryRequestPacket>(bufferReader), cancellationToken);
				case PacketType.WriteRegisterRequest:
					return await Task.Run(() => Packet.Deserialize<WriteRegisterRequestPacket>(bufferReader), cancellationToken);
				case PacketType.ReadStateRequest:
					return await Task.Run(() => Packet.Deserialize<ReadStateRequestPacket>(bufferReader), cancellationToken);
				case PacketType.WriteStateRequest:
					return await Task.Run(() => Packet.Deserialize<WriteStateRequestPacket>(bufferReader), cancellationToken);
				default:
					throw new Exception("Unknown packet type");
			}
		}

		// cancellationToken is not used
		async Task SendPacketAsync(Packet packet, CancellationToken cancellationToken)
		{
			await Task.Run(() => SendPacket(packet));
		}

		// This must be threadsafe because it can be called from loopAsync and asynchronous events
		// Could make this async but need to work around the lock, whatever
		void SendPacket(Packet packet)
		{
			byte[] buffer = Packet.Serialize(packet);
			PacketHeader hdr = new PacketHeader() { Length = (uint)buffer.Length + 8, Type = packet.Type };
			byte[] hdrbuffer = Packet.Serialize(hdr);
			lock (m_sendLock)
			{
				m_client.GetStream().Write(hdrbuffer, 0, hdrbuffer.Length);
				m_client.GetStream().Write(buffer, 0, buffer.Length);
			}
		}

		int m_id = 0;
		const uint SERVER_PACKET_FLAG = 0x80000000;
		uint GeneratePacketID()
		{
			int v = Interlocked.Increment(ref m_id);
			return SERVER_PACKET_FLAG | (uint)v;
		}
		public void OnMipsEvent(md_event e)
		{
			SendPacket(new EventNotificationPacket() { ID = GeneratePacketID(), Event = e });
		}
	}
}
