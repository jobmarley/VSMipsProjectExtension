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
using System.Globalization;

namespace MipsRemoteTest
{
	internal class Program
	{
		static bool TryParseUInt(string s, out uint value)
		{
			if (s.StartsWith("0x"))
				return uint.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
			return uint.TryParse(s, out value);
		}

		static void PrintBuffer(byte[] buffer)
		{
			int i = 0;
			while (i < buffer.Length)
			{
				int count = Math.Min(buffer.Length - i, 16);
				for (int j = 0; j < count; j++, i++)
					Console.Write(string.Format("{0:X2} ", buffer[i]));
				Console.Write("\n");
			}
		}
		static bool ParseBuffer(byte[] buffer)
		{
			int i = 0;
			while (i < buffer.Length)
			{
				string[] parts = Console.ReadLine().Split();
				foreach (string s in parts)
				{
					byte value;
					if (!byte.TryParse(s, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
					{
						Console.WriteLine("Wrong format, expected hex byte");
						return false;
					}
					buffer[i++] = value;
				}
			}
			return true;
		}
		static void Main(string[] args)
		{
			Console.WriteLine("Enter target IP:port");
			string[] endpoint = Console.ReadLine().Split(':');

			MipsRemoteDebuggerClient client = new MipsRemoteDebuggerClient();
			client.Connect(endpoint[0], int.Parse(endpoint[1]));

			client.OnMipsEvent += (sender, e) => Console.WriteLine("An event occured: " + e.Event.ToString());

			uint offset = 0;

			while (true)
			{
				string[] commandParams = Console.ReadLine().Split();
				if (commandParams.Length == 0)
					continue;

				string cmd = commandParams[0];
				if (cmd == "readreg")
				{
					if (commandParams.Length != 2)
					{
						Console.WriteLine("Wrong arg number, expected 1");
						continue;
					}

					uint register = 0;
					if (!TryParseUInt(commandParams[1], out register))
					{
						Console.WriteLine("arg 1 must be an integer");
						continue;
					}

					try
					{
						uint value = client.ReadRegisterAsync((md_register)register).Result;
						Console.WriteLine(string.Format("value: 0x{0:X8}", value));
					}
					catch (AggregateException e)
					{
						if (e.InnerException is MipsException me)
						{
							Console.WriteLine(string.Format("command failed, status: {0}", me.Status));
						}
						else
							Console.WriteLine("Exception: " + e.Message);
					}
				}
				else if (cmd == "writereg")
				{
					if (commandParams.Length != 3)
					{
						Console.WriteLine("Wrong arg number, expected 2");
						continue;
					}

					uint register = 0;
					if (!TryParseUInt(commandParams[1], out register))
					{
						Console.WriteLine("arg 1 must be an integer");
						continue;
					}
					uint value = 0;
					if (!TryParseUInt(commandParams[2], out value))
					{
						Console.WriteLine("arg 2 must be an integer");
						continue;
					}

					try
					{
						client.WriteRegisterAsync((md_register)register, value).Wait();
						Console.WriteLine(string.Format("status: ", md_status.Success));
					}
					catch (AggregateException e)
					{
						if (e.InnerException is MipsException me)
						{
							Console.WriteLine(string.Format("command failed, status: {0}", me.Status));
						}
						else
							Console.WriteLine("Exception: " + e.Message);
					}
				}
				else if (cmd == "readmem")
				{
					if (commandParams.Length != 2)
					{
						Console.WriteLine("Wrong arg number, expected 1");
						continue;
					}

					uint count = 0;
					if (!TryParseUInt(commandParams[1], out count))
					{
						Console.WriteLine("arg 1 must be an integer");
						continue;
					}

					try
					{
						byte[] buffer = new byte[count];
						uint readcount = client.ReadMemoryAsync(buffer, offset).Result;
						Console.WriteLine(string.Format("readcount: 0x{0:X8}", readcount));
						PrintBuffer(buffer);
					}
					catch (AggregateException e)
					{
						if (e.InnerException is MipsException me)
						{
							Console.WriteLine(string.Format("command failed, status: {0}", me.Status));
						}
						else
							Console.WriteLine("Exception: " + e.Message);
					}
				}
				else if (cmd == "writemem")
				{
					if (commandParams.Length != 2)
					{
						Console.WriteLine("Wrong arg number, expected 1");
						continue;
					}

					uint count = 0;
					if (!TryParseUInt(commandParams[1], out count))
					{
						Console.WriteLine("arg 1 must be an integer");
						continue;
					}

					try
					{
						byte[] buffer = new byte[count];
						if (!ParseBuffer(buffer))
							continue;

						uint writtencount = client.WriteMemoryAsync(buffer, offset).Result;
						Console.WriteLine(string.Format("writtencount: 0x{0:X8}", writtencount));
					}
					catch (AggregateException e)
					{
						if (e.InnerException is MipsException me)
						{
							Console.WriteLine(string.Format("command failed, status: {0}", me.Status));
						}
						else
							Console.WriteLine("Exception: " + e.Message);
					}
				}
				else if (cmd == "setoffset")
				{
					if (commandParams.Length != 2)
					{
						Console.WriteLine("Wrong arg number, expected 1");
						continue;
					}

					uint ofs = 0;
					if (!TryParseUInt(commandParams[1], out ofs))
					{
						Console.WriteLine("arg 1 must be an integer");
						continue;
					}

					offset = ofs;
				}
				else if (cmd == "getstate")
				{
					if (commandParams.Length != 1)
					{
						Console.WriteLine("Wrong arg number, expected 0");
						continue;
					}

					try
					{
						md_state state = client.GetStateAsync().Result;
						Console.WriteLine(string.Format("state: {0}", state));
					}
					catch (AggregateException e)
					{
						if (e.InnerException is MipsException me)
						{
							Console.WriteLine(string.Format("command failed, status: {0}", me.Status));
						}
						else
							Console.WriteLine("Exception: " + e.Message);
					}
				}
				else if (cmd == "setstate")
				{
					if (commandParams.Length != 2)
					{
						Console.WriteLine("Wrong arg number, expected 1");
						continue;
					}

					uint value = 0;
					if (!TryParseUInt(commandParams[1], out value))
					{
						Console.WriteLine("arg 1 must be an integer");
						continue;
					}

					try
					{
						client.SetStateAsync((md_state)value).Wait();
						Console.WriteLine(string.Format("status: {0}", md_status.Success));
					}
					catch (AggregateException e)
					{
						if (e.InnerException is MipsException me)
						{
							Console.WriteLine(string.Format("command failed, status: {0}", me.Status));
						}
						else
							Console.WriteLine("Exception: " + e.Message);
					}
				}
				else
				{
					Console.WriteLine("Unknown command");
					continue;
				}
			}
		}
	}
}
