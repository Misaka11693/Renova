using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renova.Core;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipWrapAttribute : Attribute
{
}
