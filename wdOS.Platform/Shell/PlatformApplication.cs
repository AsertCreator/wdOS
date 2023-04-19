using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Shell
{
    public class PlatformApplication
    {
        public void Start()
        {
            Console.WriteLine(@"
             _  ____   _____   _____  _       _    __                     
            | |/ __ \ / ____| |  __ \| |     | |  / _|                    
__      ____| | |  | | (___   | |__) | | __ _| |_| |_ ___  _ __ _ __ ___  
\ \ /\ / / _` | |  | |\___ \  |  ___/| |/ _` | __|  _/ _ \| '__| '_ ` _ \ 
 \ V  V / (_| | |__| |____) | | |    | | (_| | |_| || (_) | |  | | | | | |
  \_/\_/ \__,_|\____/|_____/  |_|    |_|\__,_|\__|_| \___/|_|  |_| |_| |_|");
            Console.WriteLine("Test App, built on wdOS Platform");
            while (true) { }
        }
    }
}
