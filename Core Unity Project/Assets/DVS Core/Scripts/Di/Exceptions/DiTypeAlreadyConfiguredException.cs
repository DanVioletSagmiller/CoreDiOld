using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvs.Core.IoC
{
    public class DiTypeAlreadyConfiguredException : DiException
    {
        public DiTypeAlreadyConfiguredException(string message) : base(message) { }
        public DiTypeAlreadyConfiguredException(string message, Exception innerException) : base(message, innerException) { }
    }
}
