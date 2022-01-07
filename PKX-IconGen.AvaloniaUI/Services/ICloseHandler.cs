using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.Services
{
    internal interface ICloseResultHandler
    {
        TResult GetResult<TResult>();
    }
}
