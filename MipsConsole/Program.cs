using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MipsConsole
{
	internal class Program
	{

		static void Main(string[] args)
		{
			MipsRemoteDebuggerUtils.MipsRemoteDebuggerClient client = new MipsRemoteDebuggerUtils.MipsRemoteDebuggerClient();

			Console.WriteLine("enter IP:port");
			var parts = Console.ReadLine().Split(':');
			if (!client.Connect(parts[0], int.Parse(parts[1])))
				throw new Exception("couldnt connect");

			// This is far from perfect
			// there will be an issue when the buffer is full, but that will do for now
			// A few ideas:
			//  - use some kind of ring buffer (when the buffer is full, the first chars are erased and the start pointer advance)
			//  - use some kind of blocking synchronization, the kernel waits for the user app to signal when it has completed read.
			//  but it won't work if the user app is not connected
			//  - same as above but with driver support, so the kernel never waits indefinitely

			uint length = 0;
			while (true)
			{
				byte[] buffer = new byte[0x1000];
				client.ReadMemoryAsync(buffer, 0x30000000).Wait();
				uint len = BitConverter.ToUInt32(buffer, 0);
				if (len > length)
				{
					string s = System.Text.Encoding.ASCII.GetString(buffer, (int)length + 4, (int)(len - length));
					Console.Write(s);
					length = len;
				}
				else if (len < length)
				{
					Console.Clear();
					string s = System.Text.Encoding.ASCII.GetString(buffer, (int)4, (int)(len));
					Console.Write(s);
					length = len;
				}
				Task.Delay(100).Wait();
			}
		}
	}
}
