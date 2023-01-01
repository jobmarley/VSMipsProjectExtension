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
using System.Collections.Concurrent;

namespace MipsRemoteDebuggerUtils
{
	public class MipsException
		: Exception
	{
		public md_status Status { get; }
		public MipsException(md_status status)
		{
			Status = status;
		}
	}
	public class MipsRemoteDebuggerClient
	{
		TcpClient m_tcpClient = null;
		ClientConnection m_clientConnection = null;
		CancellationTokenSource m_cts = null;
		Task m_receiveLoopTask = null;
		public MipsRemoteDebuggerClient()
		{

		}

		public bool Connect(string hostname, int port)
		{
			try
			{
				m_tcpClient = new TcpClient();
				m_tcpClient.Connect(IPAddress.Parse(hostname), port);
				m_clientConnection = new ClientConnection(m_tcpClient);
				m_cts = new CancellationTokenSource();
				// Task.Run to make sure its scheduled on the pool, otherwise we might deadlock if we send a packet on the same thread
				m_receiveLoopTask = Task.Run(async () => await m_clientConnection.ReceiveLoopAsync(m_cts.Token));
				m_clientConnection.OnMipsEvent += (sender, e) => OnMipsEvent?.Invoke(sender, e);
				return true;
			}
			catch(Exception e)
			{

			}
			return false;
		}

		public async Task<uint> ReadRegisterAsync(md_register r, byte sel)
		{
			var resp = await m_clientConnection.SendPacketAsync<ReadRegisterResponsePacket>(new ReadRegisterRequestPacket() { Register = r, Sel = sel }).ConfigureAwait(false);
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);

			return resp.Value;
		}
		public uint ReadRegister(md_register r, byte sel)
		{
			return ReadRegisterAsync(r, sel).Result;
		}
		public async Task WriteRegisterAsync(md_register r, byte sel, uint value)
		{
			var resp = await m_clientConnection.SendPacketAsync<WriteRegisterResponsePacket>(new WriteRegisterRequestPacket() { Register = r, Sel = sel, Value = value }).ConfigureAwait(false);
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);
		}
		public void WriteRegister(md_register r, byte sel, uint value)
		{
			WriteRegisterAsync(r, sel, value).Wait();
		}
		public async Task<uint> ReadMemoryAsync(byte[] buffer, uint offset)
		{
			var resp = await m_clientConnection.SendPacketAsync<ReadMemoryResponsePacket>(new ReadMemoryRequestPacket() { Offset = offset, Count = (uint)buffer.Length }).ConfigureAwait(false);
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);

			resp.Data.CopyTo(buffer, 0);
			return (uint)resp.Data.Length;
		}
		public uint ReadMemory(byte[] buffer, uint offset)
		{
			return ReadMemoryAsync(buffer, offset).Result;
		}
		public async Task<uint> WriteMemoryAsync(byte[] buffer, uint offset)
		{
			var resp = await m_clientConnection.SendPacketAsync<WriteMemoryResponsePacket>(new WriteMemoryRequestPacket() { Data = buffer, Offset = offset }).ConfigureAwait(false);
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);

			return resp.WrittenCount;
		}
		public uint WriteMemory(byte[] buffer, uint offset)
		{
			return WriteMemoryAsync(buffer, offset).Result;
		}
		public async Task<md_state> GetStateAsync()
		{
			var resp = await m_clientConnection.SendPacketAsync<ReadStateResponsePacket>(new ReadStateRequestPacket() { }).ConfigureAwait(false);
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);

			return resp.State;
		}
		public md_state GetState()
		{
			return GetStateAsync().Result;
		}
		public async Task SetStateAsync(md_state state)
		{
			var resp = await m_clientConnection.SendPacketAsync<WriteStateResponsePacket>(new WriteStateRequestPacket() { State = state }).ConfigureAwait(false);
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);
		}
		public void SetState(md_state state)
		{
			SetStateAsync(state).Wait();
		}

		public event EventHandler<MipsEventArgs> OnMipsEvent;

		public void Close()
		{
			m_cts.Cancel();
			// Stream.ReadAsync doesnt work properly, it doesnt react to cancel so we force close the tcp client
			m_tcpClient.Close();
			try
			{
				m_receiveLoopTask.Wait();
			}
			catch (Exception)
			{

			}
			m_tcpClient = null;
			m_cts = null;
			m_clientConnection = null;
		}
	}
}
