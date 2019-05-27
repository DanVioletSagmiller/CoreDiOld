using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Dvs.Core.IoC
{
    public abstract class DiException : Exception
    {
        public DiException(string message) : base(message) { }
        public DiException(string message, Exception innerException) : base(message, innerException) { }
        public DiException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
