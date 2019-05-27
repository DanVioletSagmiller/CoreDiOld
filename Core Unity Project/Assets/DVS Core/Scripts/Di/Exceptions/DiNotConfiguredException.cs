using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvs.Core.IoC
{
    public class DiNotConfiguredException : DiException
    {
        public DiNotConfiguredException(string message) : base(message) { }
        public DiNotConfiguredException(string message, Exception innerException) : base(message, innerException) { }
    }
}
