using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Pillow
{
    public sealed class EEExecutable
    {
        public const uint ExecutableMagic = 0xAA55AA55;
        public List<string> AllStringLiterals = new();
        public List<EEFunction> AllFunctions = new();
        public EEFunction Entrypoint;
        public int GlobalVariableCount = 0;
        public int RuntimeVersion = 1;
        public EEFunctionResult Execute(string cmd)
        {
            string[] args = cmd.Split(' ');
            List<EEObject> obj = new();
            foreach (string arg in args)
            {
                obj.Add(new EEObject(arg));
            }
            EEThread th = new(Entrypoint, obj.ToArray(), this);
            ExecutionEngine.RunningThreads.Add(th);
            ExecutionEngine.StartScheduling();
            return th.Result;
        }
    }
}
