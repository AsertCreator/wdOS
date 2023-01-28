using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Foundation;

namespace wdOS.Windows
{
    internal class Program
    {
        internal static void Main()
        {
            Console.WriteLine("Starting wdOS.Windows...");
            Kernel kernel = new Kernel();
            kernel.BeforeRun();
            Console.WriteLine("Starting wdOS is exited...");
        }
    }
}
