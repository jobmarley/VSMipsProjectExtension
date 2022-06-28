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
				m_receiveLoopTask = m_clientConnection.ReceiveLoopAsync(m_cts.Token);
				m_clientConnection.OnMipsEvent += (sender, e) => OnMipsEvent?.Invoke(sender, e);
				return true;
			}
			catch(Exception e)
			{

			}
			return false;
		}

		public async Task<uint> ReadRegisterAsync(md_register r)
		{
			var resp = await m_clientConnection.SendPacketAsync<ReadRegisterResponsePacket>(new ReadRegisterRequestPacket() { Register = r });
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);

			return resp.Value;
		}
		public async Task WriteRegisterAsync(md_register r, uint value)
		{
			var resp = await m_clientConnection.SendPacketAsync<WriteRegisterResponsePacket>(new WriteRegisterRequestPacket() { Register = r, Value = value });
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);
		}
		public async Task<uint> ReadMemoryAsync(byte[] buffer, uint offset)
		{
			var resp = await m_clientConnection.SendPacketAsync<ReadMemoryResponsePacket>(new ReadMemoryRequestPacket() { Offset = offset, Count = (uint)buffer.Length });
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);

			resp.Data.CopyTo(buffer, 0);
			return (uint)resp.Data.Length;
		}
		public async Task<uint> WriteMemoryAsync(byte[] buffer, uint offset)
		{
			var resp = await m_clientConnection.SendPacketAsync<WriteMemoryResponsePacket>(new WriteMemoryRequestPacket() { Data = buffer, Offset = offset });
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);

			return resp.WrittenCount;
		}
		public async Task<md_state> GetStateAsync()
		{
			var resp = await m_clientConnection.SendPacketAsync<ReadStateResponsePacket>(new ReadStateRequestPacket() { });
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);

			return resp.State;
		}
		public async Task SetStateAsync(md_state state)
		{
			var resp = await m_clientConnection.SendPacketAsync<WriteStateResponsePacket>(new WriteStateRequestPacket() { State = state });
			if (resp.Status != md_status.Success)
				throw new MipsException(resp.Status);
		}

		public event EventHandler<MipsEventArgs> OnMipsEvent;

		public void Close()
		{
			m_cts.Cancel();
			try
			{
				m_receiveLoopTask.Wait();
			}
			catch (Exception)
			{

			}
			m_tcpClient.Close();
			m_tcpClient = null;
			m_cts = null;
			m_clientConnection = null;
		}
	}
}
