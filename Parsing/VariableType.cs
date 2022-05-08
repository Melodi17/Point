using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Point.Parsing
{
    [Flags]
    public enum VariableType
    {
        Normal = 1,
        Local = 2,
        Global = 4,
        Dynamic = 8,
        Final = 16,
        Initializer = 32,
        Static = 64,
    }
}
