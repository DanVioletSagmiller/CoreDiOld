using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvs.Core.IoC.Tests
{
    public interface ISimpleObject
    {
        string Text { get; set; }
    }

    public class SimpleObject : ISimpleObject
    {
        public const string TextDefault = "default";
        public string Text { get; set; } = TextDefault;
    }
}
