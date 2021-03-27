using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryujinx.HLE.HOS.Tamper
{
    /// <summary>
    /// The comparisons used by conditional operations.
    /// </summary>
    enum Comparison
    {
        Greater = 1,
        GreaterOrEqual = 2,
        Less = 3,
        LessOrEqual = 4,
        Equal = 5,
        NotEqual = 6
    }
}
