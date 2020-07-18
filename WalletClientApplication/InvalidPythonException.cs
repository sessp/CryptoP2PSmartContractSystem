using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletClientApplication
{
    class InvalidPythonException : Exception
    {
        public InvalidPythonException()
        {
        }

        public InvalidPythonException(string message)
            : base(message)
        {
        }

        public InvalidPythonException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    
}
