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


	internal class Server
	{
		public Server()
		{


		}

		const int port = 16985;

		IntPtr m_devicePtr = IntPtr.Zero;

		
		void OnMipsEvent(md_event e)
		{
			Connection con = m_connection;
			if (con != null)
				Task.Run(() => con.OnMipsEvent(e));
		}
		Connection m_connection = null;
		public void Run()
		{
			TcpListener listener = new TcpListener(IPAddress.Any, port);
			Console.WriteLine(string.Format("Starting to listen on port {0}...", port));
			listener.Start();

			md_status status = mipsdebug_api.md_open(out m_devicePtr);
			status = mipsdebug_api.md_register_callback(m_devicePtr, OnMipsEvent);

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
						connection = new Connection(m_devicePtr, await listener.AcceptTcpClientAsync());
						m_connection = connection;
						Console.WriteLine(string.Format("Incoming connection from {0}", connection.Client.Client.RemoteEndPoint.ToString()));
						await connection.LoopAsync(cts.Token);
					}
					catch (Exception e)
					{
						if (connection != null)
							Console.WriteLine("Closed connection from " + connection.Client.Client.RemoteEndPoint.ToString());
					}
					m_connection = null;
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

			}

			mipsdebug_api.md_close(m_devicePtr);
		}
	}
}
