using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Foundation
{
    public class PropertyFile
    {
        public Dictionary<string, object> Properties;
        private readonly string[] properties = null;
        public PropertyFile(string content)
        {
            properties = content.Split(new char[] { ';', '\n' });
            foreach (var str in properties)
            {
                var normal = str.TrimEnd().TrimStart();
                if (!normal.StartsWith("#"))
                {
                    int i = 0;
                    string name = "";
                    string rawc;
                    while (normal[i] != '=')
                        name += normal[i++];
                    rawc = normal[i..(normal.Length - 1)];
                    if (rawc == "null") Properties[name] = null;
                    else if (rawc == "true") Properties[name] = true;
                    else if (rawc == "false") Properties[name] = false;
                    else if (long.TryParse(rawc, out long res)) Properties[name] = res;
                    else Properties[name] = rawc[1..(rawc.Length - 1)];
                }
            }
        }
    }
}
