using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProvider.AspNet
{
    internal class BootStrapper
    {
        public void Bootstrap(string name, System.Collections.Specialized.NameValueCollection config, ISqlWebProvider provider)
        {
            var database = ProviderHelper.GetAndRemove(config, "database", true);
            var throwOnError = ProviderHelper.GetAndRemoveAsBool(config, "throwOnError", false);
            var schema = ProviderHelper.GetAndRemove(config, "schema", false);
            var table = ProviderHelper.GetAndRemove(config, "table", true);

            provider.Database = database;
            provider.ThrowOnError = throwOnError ?? false;
            provider.Schema = schema ?? "dbo";
            provider.Table = table;
        }
    }
}
