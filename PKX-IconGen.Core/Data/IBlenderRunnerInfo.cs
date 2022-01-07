using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Data
{
    public interface IBlenderRunnerInfo
    {
        public string Path { get; }
        public string OptionalArguments { get; }
    }
}
