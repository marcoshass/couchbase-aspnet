using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProvider.AspNet.Session
{
    public class SqlSessionStateException : Exception
    {
        public SqlSessionStateException()
        {
        }

        public SqlSessionStateException(string message)
            : base(message)
        {
        }

        public SqlSessionStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
