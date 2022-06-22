using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MipsRemoteDebugger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Run();
        }
    }
}