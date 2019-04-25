using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dvs.Core.IoC
{
    /// <summary>
    /// Expresses that Di should not bind this field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DiSetupAttribute : Attribute
    {
        public int SortOrder = int.MinValue;
    }
}
