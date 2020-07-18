using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntermediateData1
{
    public class Job
    {
        public string pythonString;
        public string resultString;
        public int jobID;
        public int clientID;
        public Boolean jobCompleted = false;

        public override string ToString()
        {
            return "JobID:" + jobID.ToString() + " | Result: " + resultString + " | Encoded Python: " + pythonString;
        }
    }
}
