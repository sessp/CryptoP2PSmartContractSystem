using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.CompilerServices;

namespace IntermediateData1
{

    [ServiceContract]
    public interface BlockchainInterface
    {
        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        List<Block> getCurrentBlockChain();

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        Block getCurrentBlock();

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        void receiveNewTransaction(IntermediateTransaction transaction);

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        void ping();

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        void addBlock(Block b);

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        Queue<IntermediateTransaction> getTransQ();

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        void updateBlockchain(List<Block> bc);

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        void setServerID(uint id);

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        uint getServerID();

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        int getNumTransactionsToProcess();

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        IntermediateTransaction getNextTransaction();

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        List<IntermediateTransaction> getNextTransactions();
    }
}
