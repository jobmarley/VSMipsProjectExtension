/*
 Copyright (C) 2022 jobmarley

 This file is part of VSMipsProjectExtension.

 This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
 */

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
		Mutex m_deviceMutex = null;

		public TcpClient Client => m_client;
		public Connection(IntPtr devicePtr, Mutex deviceMutex, TcpClient client)
		{
			m_devicePtr = devicePtr;
			m_client = client;
			m_deviceMutex = deviceMutex;
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
				md_status status = await Task.Run(() =>
				{
					md_status s = md_status.Failure;
					try
					{
						m_deviceMutex.WaitOne();
						s = mipsdebug_api.md_read_memory(m_devicePtr, buffer, (uint)buffer.Length, rmrp.Offset, out readCount);
					}
					catch(Exception e)
					{
					}
					finally
					{
						m_deviceMutex.ReleaseMutex();
					}

					return s;
				});
				ReadMemoryResponsePacket resp = new ReadMemoryResponsePacket() { ID = rmrp.ID, Data = buffer, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is WriteMemoryRequestPacket wmrp)
			{
				uint writtenCount = 0;
				md_status status = await Task.Run(() => 
				{
					md_status s = md_status.Failure;
					try
					{
						m_deviceMutex.WaitOne();
						s = mipsdebug_api.md_write_memory(m_devicePtr, wmrp.Data, (uint)wmrp.Data.Length, wmrp.Offset, out writtenCount);
					}
					catch (Exception e)
					{
					}
					finally
					{
						m_deviceMutex.ReleaseMutex();
					}

					return s;
				});
				WriteMemoryResponsePacket resp = new WriteMemoryResponsePacket() { ID = wmrp.ID, WrittenCount = writtenCount, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is ReadRegisterRequestPacket rrrp)
			{
				uint value = 0;
				md_status status = await Task.Run(() => 
				{
					md_status s = md_status.Failure;
					try
					{
						m_deviceMutex.WaitOne();
						if (rrrp.Sel > 7)
							s = md_status.InvalidArg;
						else
							s = mipsdebug_api.md_read_register(m_devicePtr, rrrp.Register, (byte)rrrp.Sel, out value);
					}
					catch (Exception e)
					{
					}
					finally
					{
						m_deviceMutex.ReleaseMutex();
					}

					return s;
				});
				ReadRegisterResponsePacket resp = new ReadRegisterResponsePacket() { ID = rrrp.ID, Register = rrrp.Register, Value = value, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is WriteRegisterRequestPacket wrrp)
			{
				md_status status = await Task.Run(() => 
				{
					md_status s = md_status.Failure;
					try
					{
						m_deviceMutex.WaitOne();
						if (wrrp.Sel > 7)
							s = md_status.InvalidArg;
						else
							s = mipsdebug_api.md_write_register(m_devicePtr, wrrp.Register, (byte)wrrp.Sel, wrrp.Value);
					}
					catch (Exception e)
					{
					}
					finally
					{
						m_deviceMutex.ReleaseMutex();
					}

					return s;
				});
				WriteRegisterResponsePacket resp = new WriteRegisterResponsePacket() { ID = wrrp.ID, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is ReadStateRequestPacket rsrp)
			{
				md_state value = 0;
				md_status status = await Task.Run(() =>
				{
					md_status s = md_status.Failure;
					try
					{
						m_deviceMutex.WaitOne();
						s = mipsdebug_api.md_get_state(m_devicePtr, out value);
					}
					catch (Exception e)
					{
					}
					finally
					{
						m_deviceMutex.ReleaseMutex();
					}

					return s;
				});
				ReadStateResponsePacket resp = new ReadStateResponsePacket() { ID = rsrp.ID, State = value, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else if (packet is WriteStateRequestPacket wsrp)
			{
				md_status status = await Task.Run(() => 
				{
					md_status s = md_status.Failure;
					try
					{
						m_deviceMutex.WaitOne();
						s = mipsdebug_api.md_set_state(m_devicePtr, wsrp.State);
					}
					catch (Exception e)
					{
					}
					finally
					{
						m_deviceMutex.ReleaseMutex();
					}

					return s;
				});
				WriteStateResponsePacket resp = new WriteStateResponsePacket() { ID = wsrp.ID, Status = status };
				await SendPacketAsync(resp, cancellationToken);
			}
			else
				throw new Exception("Handling unknown packet type");
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

			Console.WriteLine(string.Format("Received packet, Type: {0}, Length: {1}", hdr.Type, hdr.Length));

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
					throw new Exception("Received unknown packet type");
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
			buffer = Packet.Serialize(hdr).Concat(buffer).ToArray();
			lock (m_sendLock)
			{
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
