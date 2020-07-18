using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace IntermediateData1
{
    public class Block
    {
        public uint id;
        public uint fromWalletID;
        public uint toWalletID;
        public float amount;
        public uint offset;
        public int prevHash;
        public int hash;
        public string jsonString;

    }
}