using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace P2PServer.Models
{
    //Client class, the same as Tut6.
    public class Client
    {
        
        private string ipaddress;
        private int port;
        private uint id;

        public Client(uint identificationNum,string ip, int p)
        {
            id = identificationNum;
            ipaddress = ip;
            port = p;
        }
        public string getIP()
        {
            return this.ipaddress;
        }

        public int getPort()
        {
            return this.port;
        }

        public uint getId()
        {
            return this.id;
        }

        public void setIP(string ip)
        {
            ipaddress = ip;
        }

        public void setPort(int p)
        {
            port = p;
        }

        public override string ToString()
        {
            return "\n Client: " + id + ", located at: " + ipaddress + " on Port: " + port + "\n";
        }
    }
}