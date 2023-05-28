using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Pillow
{
    public sealed class EEFunctionResult
    {
        public EEObject ReturnedValue;
        public EEObject ExceptionObject;
        public bool IsExceptionUnwinding;
    }
    public sealed class EEFunctionExecutionContext
    {
        public EEFunction Function;
        public bool IsRunning = true;
        public int InstructionPointer = 0;
        public Stack<EEObject> FunctionStack = new();
        public EEFunctionResult FunctionResult = new();
        public Stack<int> ExceptionHandlerStack = new();
        public EEObject[] Locals;
        public EEObject[] Arguments;
        public EEFunctionExecutionContext(EEFunction func, EEObject[] args)
        {
            Function = func;
            Locals = new EEObject[func.LocalCount];
            Arguments = args;
            for (int i = 0; i < func.LocalCount; i++)
            {
                Locals[i] = new(ExecutionEngine.ObjectTypeUndefined);
            }
        }
    }
    public sealed class EEFunction
    {
        public short ArgumentCount;
        public short LocalCount;
        public byte[] RawInstructions = new byte[0];
        public int AttributeAux;
        public EEFunctionAttribute Attribute;
    }
    public delegate void InstrinsicHandler(ref EEFunctionExecutionContext ctx);
}
