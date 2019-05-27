using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvs.Core.IoC
{
    public class DiConvertNotConfiguredException : DiException
    {
        public DiConvertNotConfiguredException(string message) : base(message) { }
        public DiConvertNotConfiguredException(string message, Exception innerException) : base(message, innerException) { }
    }
}
