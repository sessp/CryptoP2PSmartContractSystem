using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace P2PServer.Models
{
    public class Server
    {
        private static Server instance = null;
        private List<Client> cList;
        public static Server getWebServer()
        {
            if (instance == null)
            {
                instance = new Server();
            }
            return instance;
        }

        private Server()
        {
            cList = new List<Client>();
        }

        public Client getClient(uint id)
        {
            Client foundClient = new Client(0, "", 0);
            foreach (var c in cList)
            {
                if (c.getId() == id)
                {
                    foundClient = c;
                }
            }
            return foundClient;
        }

        public List<Client> getOtherClients(uint id)
        {
            List<Client> otherClients = new List<Client>();
            /*For each client in client list if the client is not the client requesting
             the list of other clients then add the client to the list and once iterated
             through all clients return the list*/
            foreach (var c in cList)
            {
                if (c.getId() != id)
                {
                    otherClients.Add(c);
                }
            }

            return shuffle(otherClients);
        }

        public List<Client> shuffle(IList<Client> l)
        {
            Random r = new Random();
            int n = l.Count;
            int i;
            for (i = l.Count - 1; i > 1; i--)
            {
                int random = r.Next(i + 1);
                Client c = l[random];
                l[random] = l[i];
                l[i] = c;
            }
            return (List<Client>)l;
        }

        /*For debugging and testing purposes only. */
        public void viewClientList(List<Client> list)
        {
            Debug.WriteLine("\n Contents of list: \n");
            foreach (var c in list)
            {
                Debug.WriteLine(c.ToString());
            }
        }

        public uint addClient(string ip, int port)
        {
            uint id = ((uint)cList.Count + 1);
            Client c = new Client(id, ip, port);
            Debug.WriteLine(c.ToString() + " was added to the client list by the Web Service.");
            cList.Add(c);

            return id;
        }
    }
}