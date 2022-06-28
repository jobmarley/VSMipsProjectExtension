using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace MipsRemoteDebuggerUtils
{
	public class MipsEventArgs
		: EventArgs
	{
		public md_event Event { get; }
		public MipsEventArgs(md_event e)
		{
			Event = e;
		}
	}
	internal class ClientConnection
	{
		TcpClient m_client = null;
		object m_sendLock = new object();

		public TcpClient Client => m_client;
		public ClientConnection(TcpClient client)
		{
			m_client = client;
		}

		ConcurrentDictionary<uint, TaskCompletionSource<Packet>> m_pendingPackets = new ConcurrentDictionary<uint, TaskCompletionSource<Packet>>();
		public async Task ReceiveLoopAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				Packet p = await ReceivePacketAsync(cancellationToken);
				if (p is EventNotificationPacket enp)
				{
					_ = Task.Run(() => OnMipsEvent.Invoke(this, new MipsEventArgs(enp.Event)));
				}
				else
				{
					TaskCompletionSource<Packet> tcs = null;
					if (m_pendingPackets.TryRemove(p.ID, out tcs))
						tcs.SetResult(p);
				}
			}
		}
		public async Task<TResponsePacket> SendPacketAsync<TResponsePacket>(Packet packet) where TResponsePacket : Packet
		{
			if (packet.ID == 0)
				packet.ID = GeneratePacketID();

			byte[] buffer = Packet.Serialize(packet);
			PacketHeader hdr = new PacketHeader() { Length = (uint)buffer.Length + 8, Type = packet.Type };
			byte[] hdrbuffer = Packet.Serialize(hdr);
			lock (m_sendLock)
			{
				m_client.GetStream().Write(hdrbuffer, 0, hdrbuffer.Length);
				m_client.GetStream().Write(buffer, 0, buffer.Length);
			}
			TaskCompletionSource<Packet> tcs = new TaskCompletionSource<Packet>();
			CancellationTokenSource cts = new CancellationTokenSource(20000);
			cts.Token.Register(() => tcs.TrySetCanceled());
			m_pendingPackets.TryAdd(packet.ID, tcs);
			return (TResponsePacket)(await tcs.Task);
		}

		public event EventHandler<MipsEventArgs> OnMipsEvent;

		// Doesnt need to be threadsafe since its only called from ReceiveLoopAsync
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
				case PacketType.ReadMemoryResponse:
					return await Task.Run(() => Packet.Deserialize<ReadMemoryResponsePacket>(bufferReader), cancellationToken);
				case PacketType.ReadRegisterResponse:
					return await Task.Run(() => Packet.Deserialize<ReadRegisterResponsePacket>(bufferReader), cancellationToken);
				case PacketType.WriteMemoryResponse:
					return await Task.Run(() => Packet.Deserialize<WriteMemoryResponsePacket>(bufferReader), cancellationToken);
				case PacketType.WriteRegisterResponse:
					return await Task.Run(() => Packet.Deserialize<WriteRegisterResponsePacket>(bufferReader), cancellationToken);
				case PacketType.ReadStateResponse:
					return await Task.Run(() => Packet.Deserialize<ReadStateResponsePacket>(bufferReader), cancellationToken);
				case PacketType.WriteStateResponse:
					return await Task.Run(() => Packet.Deserialize<WriteStateResponsePacket>(bufferReader), cancellationToken);
				case PacketType.EventNotificationPacket:
					return await Task.Run(() => Packet.Deserialize<EventNotificationPacket>(bufferReader), cancellationToken);
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
			return ~SERVER_PACKET_FLAG & (uint)v;
		}
	}
}
