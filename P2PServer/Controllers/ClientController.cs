using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IntermediateData;
using Microsoft.Ajax.Utilities;
using P2PServer.Models;


namespace P2PServer.Controllers
{
    public class ClientController : ApiController
    {

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        //RegisterClient - Client calls this web service to register themselves.
        [Route("api/Client/register")]
        [HttpPost]
        public uint RegisterClient([FromBody]IntermediateClient client)
        {
            Debug.WriteLine("Post request received from a client w/: " + client.ipaddress + " " + client.port + " \n");
            return Server.getWebServer().addClient(client.ipaddress, client.port);
        }

        [Route("api/Client/GetClients/{id}")]
        [HttpGet]
        public List<IntermediateClient> getClientList(uint id)
        {
            List<Client> clientList = Server.getWebServer().getOtherClients(id);
            List<IntermediateClient> cList = new List<IntermediateClient>();
            foreach (var c in clientList)
            {
                IntermediateClient client = new IntermediateClient(c.getIP(), c.getPort(), c.getId());
                cList.Add(client);

            }
            //Server.getWebServer().viewClientList(Server.getWebServer().getOtherClients(id));
            return cList;
        }
        //GetOtherClients
    }
}