using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProvider.AspNet
{
    internal interface ISqlWebProvider
    {
        string ConnectionString { get; set; }
        string Database { get; set; }
        bool ThrowOnError { get; set; }
        string Schema { get; set; }
        string Table { get; set; }
    }
}
