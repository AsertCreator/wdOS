using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Foundation
{
    internal abstract class RuntimeBase
    {
        internal abstract string Name { get; }
        internal abstract string Desc { get; }
        internal abstract int Version { get; }
    }
}
