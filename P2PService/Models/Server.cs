using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.Web;
using IntermediateData1;

namespace P2PServer.Models
{
    /*Server class, identical to Tut6, therefore not going to add any comments, if you want to know what a method does
    refer to Tut6 program*/
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
            Client foundClient = new Client(0,"",0);
            foreach(var c in cList)
            {
                if (c.getId() == id)
                {
                    foundClient = c;
                }
            }
            return foundClient;
        }

        public List<Client> getAllClients()
        {
            List<Client> otherClients = new List<Client>();
            /*For each client in client list if the client is not the client requesting
             the list of other clients then add the client to the list and once iterated
             through all clients return the list*/
            //check if they are still active
            foreach (var c in cList)
            {
                try
                {

                    ChannelFactory<BlockchainInterface> bcInterface;
                    NetTcpBinding tcp = new NetTcpBinding();
                    string url = String.Format("net.tcp://localhost:{0}/bService", c.getPort());
                    bcInterface = new ChannelFactory<BlockchainInterface>(tcp, url);
                    BlockchainInterface bcServer = bcInterface.CreateChannel();
                    bcServer.ping();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("\n Exception: One of the clients is no longer responding - The client will be removed from the client list" + exception.Message);
                    cList.Remove(c);
                }
            }    
            foreach (var c in cList)
            {
                
                    otherClients.Add(c);
                
            }

            return shuffle(otherClients);
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
            uint id = 0;
            IPAddress r = null;
          if (IPAddress.TryParse(ip, out r)) 
          { 
             id = ((uint)cList.Count + 1);
            Client c = new Client(id,ip,port);
            Debug.WriteLine(c.ToString() + " was added to the client list by the Web Service.");
            cList.Add(c);
                //TODO:Fix these IP Address Validation
          }
    
            return id;
        }
    }
}