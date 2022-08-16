using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core
{
    internal static class PathUtils
    {
        internal static string CanonicalPath(bool doubleup, params string[] xpath)
        {
            List<string> alldirs = xpath.ToList();
            List<string> finaldirs = new();
            if (alldirs[0].EndsWith(':'))
            {
                finaldirs.Add(alldirs[0]);
                foreach (var dir in alldirs)
                {
                    if (dir.Contains("..") && dir.Length > 2)
                    {
                        finaldirs.RemoveAt(finaldirs.Count - 1);
                        if (doubleup)
                            finaldirs.RemoveAt(finaldirs.Count - 1);
                        continue;
                    }
                    switch (dir.Trim())
                    {
                        case "..":
                            if (finaldirs.Count > 2)
                            {
                                finaldirs.RemoveAt(finaldirs.Count - 1);
                                if (doubleup)
                                    finaldirs.RemoveAt(finaldirs.Count - 1);
                            }
                            break;
                        case ".": break;
                        default: finaldirs.Add(dir); break;
                    }
                }
            }
            Kernel.Log(string.Join("\\", finaldirs));
            return string.Join("\\", finaldirs);
        }
    }
}
