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

using MipsRemoteDebuggerUtils;

namespace MipsRemoteDebugger
{


	internal class Server
	{
		public Server()
		{


		}

		const int port = 16985;

		IntPtr m_devicePtr = IntPtr.Zero;

		
		void OnMipsEvent(md_event e)
		{
			foreach (Connection con in m_connections.Values)
			{
				if (con != null)
					Task.Run(() => con.OnMipsEvent(e));
			}
		}
		ConcurrentDictionary<TcpClient, Connection> m_connections = new ConcurrentDictionary<TcpClient, Connection>();
		Mutex m_deviceMutex = new Mutex();

		public void Run()
		{
			try
			{
				md_status status = mipsdebug_api.md_open(out m_devicePtr);
				if (status != md_status.Success)
					throw new Exception(string.Format("Could not open a handle to the device, status: {0}", status));

				status = mipsdebug_api.md_register_callback(m_devicePtr, OnMipsEvent);
				if (status != md_status.Success)
					throw new Exception(string.Format("Could not register callback, status: {0}", status));

				TcpListener listener = new TcpListener(IPAddress.Any, port);
				Console.WriteLine(string.Format("Starting to listen on port {0}...", port));
				listener.Start();

				CancellationTokenSource cts = new CancellationTokenSource();

				Task loopTask = Task.Run(async () =>
				{
				// This loop services 1 connection at a time
					while (!cts.Token.IsCancellationRequested)
					{
						Connection connection = null;
						try
						{
						// AcceptTcpClientAsync cannot be canceled, it will raise an exception when listener stops
							TcpClient client = await listener.AcceptTcpClientAsync();
							connection = new Connection(m_devicePtr, m_deviceMutex, client);
							m_connections.TryAdd(client, connection);
							Console.WriteLine(string.Format("Incoming connection from {0}", connection.Client.Client.RemoteEndPoint.ToString()));
							_ = Task.Run(async () =>
							{
								await connection.LoopAsync(cts.Token);
								m_connections.TryRemove(client, out _);
							});
						}
						catch (Exception e)
						{
							Console.WriteLine("Exception: " + e.Message);
							if (connection != null)
								Console.WriteLine("Closed connection from " + (connection?.Client?.Client?.RemoteEndPoint?.ToString() ?? "<null>"));
						}
					}
				}, cts.Token);

				while (true)
				{
					Console.Write(">> ");
					string s = Console.ReadLine();
					if (s == "exit")
					{
						break;
					}
				}

				// unregister before closing to make sure we dont send events to a closed connection, but we dont really care
				status = mipsdebug_api.md_unregister_callback(m_devicePtr, OnMipsEvent);

				try
				{
					cts.Cancel();
					// Need to stop listening to stop AcceptTcpClientAsync
					listener.Stop();
					loopTask.Wait();
				}
				catch (Exception)
				{
					// thats not important
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(string.Format("Exception: {0}", e.Message));
			}
			if (m_devicePtr != IntPtr.Zero)
				mipsdebug_api.md_close(m_devicePtr);
		}
	}
}
