using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IntermediateData1;
using Newtonsoft.Json;
using RestSharp;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Windows.Forms;
//TEst 
namespace WalletClientApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String ip = "127.0.0.1";
        private int initPort = 8000;
        private BlockchainInterface bcServer;
        private ServiceHost host;
        private Boolean available;
        private uint id;
        private int jobID = 0;
        private RestClient client;
        private bool enabled = true;
        private Boolean runningBackgroundThread;
        private object _lockRunning = 0;
        private List<Job> thisClientsJobList = new List<Job>(); //Unique for each client, we are only tracking/displaying jobs + solutions for this client.

        public MainWindow()
        {
            InitializeComponent();
            runningBackgroundThread = false;

            startup();
            startJMiner();
        }

        //Start up method
        private void startup()
        {
            try
            {
                //Create server, register client, connect and set the ID, then create the blockchain
                createServer(ip, initPort);
                registerClient();
                connect(ip, initPort);
                bcServer.setServerID(id);

                createBlockchain();
            }
            catch(ServerUnavailableException exception)
            {
                disableInput();
            }
        }

        //Register client w/ P2P Server. Typical register from Tut6.
        private void registerClient()
        {
            try
            {
                Debug.WriteLine("\n =============== Register Client ================ \n");
                client = new RestClient("https://localhost:44304/");
                IntermediateClient interClient = new IntermediateClient("127.0.0.1", initPort, 0);
                RestRequest r = new RestRequest("api/Client/register");
                r.AddJsonBody(interClient);
                IRestResponse response = client.Post(r);
                Debug.WriteLine("\n Received Client ID from web service: " + response.Content);
                id = UInt32.Parse(response.Content);
            }
            catch(System.FormatException exception)
            {
                //disableInput();
                errorOutput.Content = " An error occured while attempting to connect to the P2P Server, please close and reopen your application ";
                throw new ServerUnavailableException(" An error occured while attempting to connect to the P2P Server, please close and reopen your application ");


            }
        }

        //Disable input for error handling, if the need arises.
        private void disableInput()
        {
            if (enabled == true)
            {
                button_submitPython.IsEnabled = false;
                pythonInput.IsEnabled = false;
            }
            else
            {
                button_submitPython.IsEnabled = true;
                pythonInput.IsEnabled = true;
            }


        }

        //Method to create the blockchain
        private void createBlockchain()
        {
            //Get clients.
            RestRequest r = new RestRequest("api/Client/GetClients/" + id);
            IRestResponse response = client.Get(r);
            List<IntermediateClient> otherClients = JsonConvert.DeserializeObject<List<IntermediateClient>>(response.Content);
            //If we are the first and only client then we can create the block client.
            if (otherClients.Count == 0)
            {
                Debug.WriteLine("\n We are the first client, therefore we must create the blockchain");
                connect(ip, initPort);
                bcServer.ping();
                IntermediateData1.Block b = createInitJBlock();
                bcServer.addBlock(b);
            }
            else
            {
                /*If someone has already created the block client, lets get it off them rather
                than create a new one*/
                Debug.WriteLine("\n There is already another client, get their blockchain!");
                IntermediateClient neighbourClient = otherClients.First();
                connect(neighbourClient.ipaddress, neighbourClient.port);
                List<IntermediateData1.Block> neighbourBC = bcServer.getCurrentBlockChain();
                connect(ip, initPort);
                bcServer.updateBlockchain(neighbourBC);

            }



        }

        

        //Create server method.
        private void createServer(string ipAddress, int port)
        {
            int p;
            p = port;
            while (!available)
            {
                try
                {
                    //Loop until we find an available port.
                    p = initPort; 
                    NetTcpBinding tcp = new NetTcpBinding();

                    host = new ServiceHost(typeof(BlockchainServer));
                    
                    string url = String.Format("net.tcp://0.0.0.0:{0}/bService", p);
                    host.AddServiceEndpoint(typeof(BlockchainInterface), tcp, url);
                    available = true;
                    initPort = p;
                    host.Open();

                }
                catch (Exception e)
                {
                    //Port not available? Increment and try again.
                    Debug.WriteLine("\n Exception: " + e.Message);
                    available = false;
                    initPort++;
                }
            }
        }

        //Connect to remote server method.
        public void connect(string ip, int port)
        {
            try
            {

                ChannelFactory<BlockchainInterface> bcInterface;
                NetTcpBinding tcp = new NetTcpBinding();
                string url = String.Format("net.tcp://localhost:{0}/bService", port);
                bcInterface = new ChannelFactory<BlockchainInterface>(tcp, url);
                bcServer = bcInterface.CreateChannel();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("\n Exception: " + exception.Message);
            }
        }
        /*-----------------------------New CODE FOR TUT 9/Not 'directly' taken from Tut6----------------------*/
        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (host != null)
            {
                host.Close();
            }
            Debug.WriteLine("\n Window Closing :( \n");
        }

        //Encoding and decoding a base 64 string/python string.
        public string encodeBase64(String pythonString)
        {
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(pythonString);
            return Convert.ToBase64String(textBytes);
        }
        public string decodeBase64(String base64Text)
        {
            byte[] encodedBytes = Convert.FromBase64String(base64Text);
            return System.Text.Encoding.UTF8.GetString(encodedBytes);
        }

        //Method executed when the associated button is clicked.
        private void button_submitPython_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pythonInput.Text != null)
                {
                    String pythonToRun = pythonInput.Text;
                    jobID++;
                    /*Get the string, put it in a job object, add it to this clients job list
                    then update display and broadcast to other clients.*/
                    Job j = new Job();
                    j.clientID = (int)id;
                    j.jobCompleted = false;
                    j.jobID = jobID;
                    j.pythonString = encodeBase64(pythonToRun);
                    j.resultString = "";
                    thisClientsJobList.Add(j);
                    updateDisplay();
                    broadcastTransaction(j.pythonString);
                }
            }
            catch (Exception except)
            {
                Debug.WriteLine(except.Message);
            }
}
        /*Method is used to broadcast to other clients.*/
        private void broadcastTransaction(string s)
        {
            IntermediateTransaction t = new IntermediateTransaction();
            t.python = s;
            string url = "https://localhost:44304/";
            RestClient c = new RestClient(url);

            RestRequest r1 = new RestRequest("api/Client/GetClientsAll/");
            IRestResponse response = c.Get(r1);

            List<IntermediateClient> clientList = JsonConvert.DeserializeObject<List<IntermediateClient>>(response.Content);
            //Get a list of other clients
            foreach (var otherClient in clientList)
            {
                //Connect to them
                connect(otherClient.ipaddress, otherClient.port);
                bcServer.receiveNewTransaction(t);
                //Give them the new transaction.
            }
        }

        //Method run by the Miner thread.
        private void startJMiner()
        {
                new Thread(delegate ()
                {
                    try
                    {
                        Debug.WriteLine(" Mining Thread is running for client {0} is running with ID: " + Thread.CurrentThread.ManagedThreadId, id);
                        connect(ip, initPort); //connect to this server
                        while (true)
                        {
                            //Wait for 5 transactions to occur
                            connect(ip, initPort);
                            if (bcServer.getNumTransactionsToProcess() >= 5)// > or >=?
                            {
                                
                                //Organising them alphabetically is done in server.

                                mineJobs(bcServer.getNextTransactions());


                                //isUpdated();


                                checkResult();
                                updateDisplay();
                                //Update the GUI.
                                //Is it up to date?!?!
                            }
                            else
                            {
                                //wait

                            }
                        }
                    }
                    catch (System.ServiceModel.EndpointNotFoundException e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.Message, "Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //close the program and display error.
                    }
                    catch (InvalidPythonException e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //Close the current thread/delegate
                    }

                }).Start();
        }

        private void checkResult()
        {
            //Check with your server if it any jobs are complete
            
            connect(ip, initPort);
            List<IntermediateData1.Block> blockList = bcServer.getCurrentBlockChain();
            /* Get the blockList, for each block in block list get their jsonList/string
             for each entry in that string get compare that with all the jobs in the clients job list
             and if an entry equals a jobstring then they are the same and if there is a result then set the result
             string.*/
            foreach (var b in blockList)
            {
                List<string[]> jsonList = JsonConvert.DeserializeObject<List<string[]>>(b.jsonString);
                //skip first initial block
                if (jsonList != null)
                {
                    foreach (var e in jsonList)
                    {
                        foreach (var j in thisClientsJobList)
                        {
                            if (e[0].Equals(j.pythonString) && !e[1].Equals(""))
                            {
                                j.resultString = e[1];
                            }
                        }
                    }
                }
            }
        }

        //Update the display.
        private void updateDisplay()
        {
            //Display
            Dispatcher.BeginInvoke(
                new Action(() => {
                    if (thisClientsJobList.Count != 0)
                    {
                        board.Items.Clear();
                        foreach (var j in thisClientsJobList)
                        {
                            Job jDisplay = new Job();
                            jDisplay.jobID = j.jobID;
                            jDisplay.clientID = j.clientID;
                            jDisplay.jobCompleted = j.jobCompleted;
                            jDisplay.pythonString = decodeBase64(j.pythonString);
                            jDisplay.resultString = decodeBase64(j.resultString);
                            board.Items.Add(jDisplay.ToString());
                        }
                    }
                })
               );
            
        }

        public void mineJobs(List<IntermediateTransaction> transList)
        {
            try
            {
                List<string[]> jsonList = new List<string[]>();
                foreach (var t in transList)
                {
                    //Decode
                    string pyString = decodeBase64(t.python);
                    string[] sArray = new string[2];
                    //Run Python + Store result in string s.  Test using: round(x),pow(x,y)
                    ScriptEngine engine = Python.CreateEngine();
                    ScriptScope scope = engine.CreateScope();
                    dynamic pythonCode = engine.Execute(pyString, scope);
                    var result = pythonCode;
                    string s = result.ToString();
                    //Get result, encode it and put it onto the jsonList.
                    t.answer = encodeBase64(s);
                    sArray[0] = t.python;
                    sArray[1] = t.answer;
                    
                    jsonList.Add(sArray);
                }
                //Serialize it.
                String jsonString = JsonConvert.SerializeObject(jsonList);
                Debug.WriteLine("\n" + jsonString + "\n");

                //Generate Block
                IntermediateData1.Block b = constructBlock(jsonString);

                IntermediateData1.Block prevBlock = bcServer.getCurrentBlock();
                uint miningOffset;

                //Generate Hash
                b.prevHash = prevBlock.hash;
                miningOffset = 0;

                //Keep generating hash until it starts with 12345
                int hash = generateJobHash(b, miningOffset);
                while (!(hash.ToString().StartsWith("12345")))
                {
                    miningOffset += 1;
                    hash = generateJobHash(b, miningOffset);
                }
                Debug.WriteLine("\n Final Hash Generated By Client {0} - " + hash + " \n", id);
                b.hash = hash;
                b.offset = miningOffset;
                //Connect to this remote server and addBlock.
                connect(ip, initPort);
                bcServer.addBlock(b);
            }
            catch (ArgumentNullException exception)
            {
                throw new InvalidPythonException(exception.Message);
            }
            catch (Microsoft.Scripting.SyntaxErrorException exception)
            {
                throw new InvalidPythonException(exception.Message);
            }
            catch (NotSupportedException exception)
            {
                throw new InvalidPythonException(exception.Message);
            }
            catch (IronPython.Runtime.UnboundLocalException exception)
            {
                throw new InvalidPythonException(exception.Message);
            }
            catch (FormatException exception)
            {
                throw new InvalidPythonException(exception.Message);
            }
            catch (IronPython.Runtime.UnboundNameException exception)
            {
                throw new InvalidPythonException(exception.Message);
            }
        }

        //Construct a block
        private IntermediateData1.Block constructBlock(string jsonString)
        {
            connect(ip, initPort);
            IntermediateData1.Block b = new IntermediateData1.Block();
            
            b.id = 1 + bcServer.getCurrentBlock().id;
            b.jsonString = jsonString;
            return b;
        }

        //Generate the job hash.
        private int generateJobHash(IntermediateData1.Block b, uint offset)
        {
            string concatString = b.id.ToString() + b.jsonString + offset.ToString() + b.prevHash.ToString();
            SHA256 sha256Hash = SHA256.Create();
            byte[] h = sha256Hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(concatString));
            int hash = BitConverter.ToInt32(h, 0);
            return hash;
        }

        /*Check if is updated. I've commented it out of the actual program but it works so if you want
        You can uncomment it. The reason it is commented is because tut9 didn't mention anything about making sure
        the blockchains are up to date/the need for this method.
             */
        private void isUpdated()
        {
            string url = "https://localhost:44304/";
            RestClient c = new RestClient(url);

            RestRequest r1 = new RestRequest("api/Client/GetClients/" + id);
            IRestResponse response = c.Get(r1);

            List<IntermediateClient> clientList = JsonConvert.DeserializeObject<List<IntermediateClient>>(response.Content);

            //Because it's a list of other clients, not all clients must be > 0 and not 1.
            if (clientList.Count() > 0)
            {
                List<IntermediateData1.Block> listOfCurrentBlocks = new List<IntermediateData1.Block>();
                Dictionary<int, IntermediateClient> popularityList = new Dictionary<int, IntermediateClient>();
                foreach (var otherClient in clientList)
                {
                    //Connect to them
                    connect(otherClient.ipaddress, otherClient.port);
                    //Figure out most popular blockchain
                    popularityList.Add(bcServer.getCurrentBlock().hash, otherClient);
                    listOfCurrentBlocks.Add(bcServer.getCurrentBlock());
                }

                //Some cool code to figure out the most popular hash. Using querying.
                var q = listOfCurrentBlocks.GroupBy(x => x.hash)
                        .Select(group => new { h = group.Key, count = group.Count() })
                        .OrderByDescending(x => x.count);
                var mostPopular = q.First();
                Debug.WriteLine("\n" + " Most Popular found by Client {0}: " + mostPopular.ToString() + "\n", id.ToString());
                
                connect(ip, initPort);
                if (bcServer.getCurrentBlock().hash == mostPopular.h)
                {
                    //No need to update the blockchain, hurray !
                    Debug.WriteLine("\n Most Popular matches the current block in Client/Server {0} \n", id);
                }
                else
                {
                    //Update the blockchain with the popular one.
                    Debug.WriteLine("\n Need to update the blockchain! \n", id);
                    IntermediateClient clientToCopyFrom;
                    popularityList.TryGetValue(mostPopular.h, out clientToCopyFrom);
                    connect(clientToCopyFrom.ipaddress, clientToCopyFrom.port);
                    List<IntermediateData1.Block> blockChainToCopy = bcServer.getCurrentBlockChain();
                    connect(ip, initPort);
                    bcServer.updateBlockchain(blockChainToCopy);

                }
                connect(ip, initPort);
            }
            else
            {
                Debug.WriteLine("There are no other clients to check synchronization with!");
            }

        }

        //Method to create the initial block.
        private IntermediateData1.Block createInitJBlock()
        {
            
            IntermediateData1.Block initBlock = new IntermediateData1.Block();
            initBlock.id = 1;
            initBlock.jsonString = " ";
            initBlock.offset = 1;
            initBlock.prevHash = 0;

            int hash = generateJobHash(initBlock, initBlock.offset);
            while (!(hash.ToString().StartsWith("12345"))) 
            {
                initBlock.offset += 1;
                hash = generateJobHash(initBlock, initBlock.offset);
            }
            Debug.WriteLine("\n The hash of init is: {0}", hash);

            initBlock.hash = hash;

            return initBlock;
        }
    }
}
