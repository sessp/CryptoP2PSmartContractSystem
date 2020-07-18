using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntermediateData1
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext
= false)]
    //Blockchain server class.
    public class BlockchainServer : BlockchainInterface
    {
        //Local and static variables.
        private static List<Block> blockChain = new List<Block>();
        private static Queue<IntermediateTransaction> transactionQ = new Queue<IntermediateTransaction>();
        private static uint serverID = 0;
        //Track server ID, for debugging purposes.

        public BlockchainServer()
        {

        }

        public Block getCurrentBlock()
        {
            return blockChain.Last();
        }

        public List<Block> getCurrentBlockChain()
        {
            return blockChain;
        }

        //If blockchain is outdated, update it :D
        public void updateBlockchain(List<Block> bc)
        {
            blockChain = bc;
            Debug.WriteLine("\n  BlockChain has been updated for Server {0}! \n", serverID);
        }

        //Called by the client when a new transaction is received.
        public void receiveNewTransaction(IntermediateTransaction transaction)
        {
            Debug.WriteLine("\nServer {0} has received Transaction:" + transaction.print() + "\n", serverID);
            transactionQ.Enqueue(transaction);
        }

        public void addBlock(Block b)
        {
            submitBlock(b);
        }

        public Queue<IntermediateTransaction> getTransQ()
        {
            return transactionQ;
        }

        public IntermediateTransaction getNextTransaction()
        {
            return transactionQ.Dequeue();
        }

        //Get next transactions - Gets the next 5 transactions.
        public List<IntermediateTransaction> getNextTransactions()
        {
            List<IntermediateTransaction> fiveTransactions = new List<IntermediateTransaction>();
            //Make sure there are actually 5.
            if (transactionQ.Count >= 5)
            {
                int i = 0;
                while (i < 5)
                {
                    fiveTransactions.Add(transactionQ.Dequeue());
                    i++;
                }
                //Organise in alphabetical order
                
                var alphabeticallySorted = fiveTransactions.OrderBy(x => x.python).ToList<IntermediateTransaction>();

                fiveTransactions = alphabeticallySorted;
            }
            else
            {
                Debug.WriteLine("Error, Transaction Queue doesn't contain >= 5");
                //Debugging
                foreach (var t in transactionQ.ToList())
                {
                    Debug.WriteLine(" Printing Tq: " + t.ToString());
                }
            }
            return fiveTransactions; 
        }

        //Used for pinging + testing the initial connection for a server. 
        public void ping()
        {
           
        }

        public void setServerID(uint id)
        {
            serverID = id;
        }

        public uint getServerID()
        {
            return serverID;
        }

        public int getNumTransactionsToProcess()
        {
            return transactionQ.Count();
        }

        /*Performs all the various checks for validity for a block. Called before a block is added on a potential block.*/
        /*Taken from TUT 7*/
        private Boolean isValid(Block blockToSubmit, out string validityMsg)
        {
            bool isValid = true;
            validityMsg = " ";
            //Check 1
            foreach (var b in blockChain)
            {
                if (b.id > blockToSubmit.id)
                {
                    isValid = false;
                    validityMsg = "Error with Block Structure: Block ID is not valid ";
                }
            }

            //Check 4
            if (blockToSubmit.offset < 0)
            {
                isValid = false;
                validityMsg = "Error with Block Structure: Block offset is not > 0 ";
            }

            //If it isn't the initial/first block 
            if (blockChain.Count != 0)
            {
                //check 5
                if (blockToSubmit.prevHash != blockChain.Last().hash)
                {
                    isValid = false;
                    validityMsg = "Error with Block Structure: Previous Hash value in the block is not valid! ";
                }
                //Check 7 
                if (blockToSubmit.hash != generateJHash(blockToSubmit))
                {
                    isValid = false;
                    validityMsg = "Error with Block Structure: Invalid Hash value, hash is incorrect! ";
                }
            }


            //check 6
            if (!(blockToSubmit.hash.ToString().StartsWith("12345")))
            {
                isValid = false;
                validityMsg = "Error with Block Structure: Invalid Hash value, it should start with 12345! ";
            }

            return isValid;
        }

        //Method generates a hash, used by block validation.
        private int generateJHash(Block b)
        {
            SHA256 sha256Hash = SHA256.Create();
            string concatString = b.id.ToString() + b.jsonString + b.offset.ToString() + b.prevHash.ToString();
            byte[] h = sha256Hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(concatString));
            int hash = BitConverter.ToInt32(h, 0);
            return hash;
        }

        //Method to submit a block.
        private void submitBlock(Block blockToSubmit)
        {
            string output = "";
            //Check is valid
            if (isValid(blockToSubmit, out output))
            {
                //Add to blockchain
                blockChain.Add(blockToSubmit);
                
                //Print debugging info
                Debug.WriteLine("\n  Block was added to Server {0}! \n", serverID);
                Debug.WriteLine("\n Current Blocks in the chain: " + getState());
                //Loop through blockchain and debug
                foreach (var b in blockChain)
                {
                    Debug.WriteLine("\n  Block ID:{0} Offset:{1} prevHash:{2} hash:{3} jsonString:{4}  \n", b.id, b.offset, b.prevHash, b.hash, b.jsonString);
                }    
                
            }
            else
            {
                Debug.WriteLine("\n" + output + "\n");
            }
        }

        private int getState()
        {
            return blockChain.Count;
        }

    }
}
