using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntermediateData1
{
    //Intermediate Client class.
    public class IntermediateClient
    {
        public string ipaddress;
        public int port;
        public uint id;

        public IntermediateClient(string ip, int p, uint identification)
        {
            ipaddress = ip;
            port = p;
            id = identification;
        }
    }
}
