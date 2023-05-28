using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Pillow
{
    public class EEThread
    {
        public EEFunction StartFunction;
        public Stack<EEFunctionExecutionContext> Contexts;
        public EEFunctionResult Result;
        public bool DumpStack;
        public EEExecutable ExecutableContext;
        public EEThread(EEFunction func, EEObject[] args, EEExecutable exec)
        {
            StartFunction = func;
            Contexts = new();
            Contexts.Push(new(func, args));
            ExecutableContext = exec;
        }
        public static void ThrowAnException(EEObject obj,
            ref EEFunctionResult funcres, ref Stack<int> exchandlerstack, ref Stack<EEObject> stack,
            ref bool running, ref int i)
        {
            if (exchandlerstack.Count > 0)
            {
                i = exchandlerstack.Pop();
                stack.Push(obj);
            }
            else
            {
                running = false;
                stack.Push(new EEObject(ExecutionEngine.ObjectTypeNull));
                funcres.ExceptionObject = obj;
                funcres.IsExceptionUnwinding = true;
            }
        }
        internal void Step(ref EEFunctionExecutionContext ctx, bool dumpstack)
        {
            EEObject tempobj0;
            EEObject tempobj1;

            if (dumpstack)
            {
                Console.WriteLine($"IL{ctx.InstructionPointer:X8}: " +
                    @$"{EEDisassembler.DisassembleInstruction(
                        ctx.Function.RawInstructions[
                            ctx.InstructionPointer..(ctx.InstructionPointer +
                            EEDisassembler.GetInstructionSize(
                                ctx.Function.RawInstructions[ctx.InstructionPointer]))])}");
            }
            switch (ctx.Function.RawInstructions[ctx.InstructionPointer])
            {
                case 0x00: // nop
                    ctx.InstructionPointer += 1;
                    break;
                case 0x01: // ret
                    ExecutionEngine.CollectGarbage();
                    ctx.IsRunning = false;
                    ctx.InstructionPointer += 1;
                    ctx.FunctionResult = new();
                    ctx.FunctionResult.ReturnedValue = ctx.FunctionStack.Pop();
                    break;
                case 0x02: // add
                    tempobj0 = ctx.FunctionStack.Pop();
                    tempobj1 = ctx.FunctionStack.Pop();
                    ctx.FunctionStack.Push(tempobj0 + tempobj1);
                    ctx.InstructionPointer += 1;
                    break;
                case 0x03: // sub
                    tempobj0 = ctx.FunctionStack.Pop();
                    tempobj1 = ctx.FunctionStack.Pop();
                    ctx.FunctionStack.Push(tempobj0 - tempobj1);
                    ctx.InstructionPointer += 1;
                    break;
                case 0x04: // mul
                    tempobj0 = ctx.FunctionStack.Pop();
                    tempobj1 = ctx.FunctionStack.Pop();
                    ctx.FunctionStack.Push(tempobj0 * tempobj1);
                    ctx.InstructionPointer += 1;
                    break;
                case 0x05: // div
                    tempobj0 = ctx.FunctionStack.Pop();
                    tempobj1 = ctx.FunctionStack.Pop();
                    ctx.FunctionStack.Push(tempobj0 / tempobj1);
                    ctx.InstructionPointer += 1;
                    break;
                case 0x06: // pushint
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeInteger)
                    {
                        ObjectValue = BitConverter.ToInt64(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 9)])
                    });
                    ctx.InstructionPointer += 9;
                    break;
                case 0x07: // pushfalse
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeBoolean)
                    {
                        ObjectValue = 0L
                    });
                    ctx.InstructionPointer += 1;
                    break;
                case 0x08: // pushtrue
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeBoolean)
                    {
                        ObjectValue = 1L
                    });
                    ctx.InstructionPointer += 1;
                    break;
                case 0x09: // pushnull
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeNull));
                    ctx.InstructionPointer += 1;
                    break;
                case 0x0A: // pushundf
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeUndefined));
                    ctx.InstructionPointer += 1;
                    break;
                case 0x0B: // pushstr
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeString)
                    {
                        ObjectValue = ExecutableContext.AllStringLiterals[BitConverter.ToInt32(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)])]
                    });
                    ctx.InstructionPointer += 5;
                    break;
                case 0x0D: // pushobj
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeCompound));
                    ctx.InstructionPointer += 1;
                    break;
                case 0x0E: // pushfunc
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeFunction)
                    {
                        ObjectValue = ExecutableContext.AllFunctions[BitConverter.ToInt32(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)])]
                    });
                    ctx.InstructionPointer += 5;
                    break;
                case 0x0F:
                    { // setfield
                        var value = ctx.FunctionStack.Pop();
                        var key = ctx.FunctionStack.Pop();
                        var compound = ctx.FunctionStack.Pop();
                        if (compound.ObjectType != ExecutionEngine.ObjectTypeCompound)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to set field to object, which is not compound"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        var dict = (CustomDictionary)compound.ObjectValue;

                        if (dict.Keys.Contains(key))
                            dict.GetValue(key).References--;

                        dict.SetKeyValue(key, value);
                        value.References++;
                        ctx.FunctionStack.Push(compound);
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x10:
                    { // getfield
                        var key = ctx.FunctionStack.Pop();
                        var compound = ctx.FunctionStack.Pop();
                        if (compound.ObjectType != ExecutionEngine.ObjectTypeCompound)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to get value of field of object, which is not compound"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        var dict = (CustomDictionary)compound.ObjectValue;

                        if (!dict.Keys.Contains(key)) ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeUndefined));
                        else ctx.FunctionStack.Push(dict.GetValue(key));
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x11:
                    { // setlocal
                        var local = ctx.FunctionStack.Pop();
                        var value = ctx.FunctionStack.Pop();

                        if (local.ObjectType != ExecutionEngine.ObjectTypeInteger)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to set value to argin, which index is not an integer"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);
                        var index = (long)local.ObjectValue;

                        if (index < 0 || index > ctx.Function.LocalCount)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to set value to non-existent argin"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        value.References++;
                        ctx.Locals[index].References--;
                        ctx.Locals[index] = value;
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x12:
                    { // getlocal
                        var local = ctx.FunctionStack.Pop();
                        if (local.ObjectType != ExecutionEngine.ObjectTypeInteger)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to get value of argin, which index is not an integer"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);
                        var index = (long)local.ObjectValue;

                        if (index < 0 || index > ctx.Function.LocalCount)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to get value of non-existent argin"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        ctx.FunctionStack.Push(ctx.Locals[index]);
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x13:
                    { // pusheh
                        var local = ctx.FunctionStack.Pop();

                        if (local.ObjectType != ExecutionEngine.ObjectTypeInteger)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to set exception handler label, which is not an integer"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);
                        var index = (long)local.ObjectValue;

                        if (index < 0 || index > ctx.Function.RawInstructions.Length)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to set non-existent exception handler label"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        ctx.ExceptionHandlerStack.Push((int)index);

                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x14:
                    { // popeh
                        ctx.ExceptionHandlerStack.Pop();
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x15:
                    { // throw
                        ThrowAnException(ctx.FunctionStack.Pop(), ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x16:
                    { // deletefield
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x17:
                    { // call
                        var func = ctx.FunctionStack.Pop();
                        if (func.ObjectType != ExecutionEngine.ObjectTypeFunction)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to call an object, which is not a function"
                            }, 
                            ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        EEFunction fact = (EEFunction)func.ObjectValue;

                        if (fact.Attribute != EEFunctionAttribute.Instrinsic)
                        {
                            List<EEObject> list = new();
                            for (int j = 0; j < fact.ArgumentCount; j++)
                                list.Add(ctx.FunctionStack.Pop());

                            EEFunctionExecutionContext nctx = new(fact, list.ToArray());

                            Contexts.Push(nctx);
                        }
                        else ExecutionEngine.InstrinsicHandlers[fact.AttributeAux](ref ctx);

                        ctx.InstructionPointer += 1;
                        return;
                    }
                case 0x18:
                    { // br
                        ctx.InstructionPointer = (int)BitConverter.ToInt64(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)]);
                        break;
                    }
                case 0x19:
                    { // breq
                        tempobj0 = ctx.FunctionStack.Peek();
                        tempobj1 = ctx.FunctionStack.Peek();
                        if (tempobj0.ObjectValue == tempobj1.ObjectValue)
                            ctx.InstructionPointer = (int)BitConverter.ToInt64(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)]);
                        else
                            ctx.InstructionPointer += 5;
                        break;
                    }
                case 0x1A:
                    { // brneq
                        tempobj0 = ctx.FunctionStack.Peek();
                        tempobj1 = ctx.FunctionStack.Peek();
                        if (tempobj0.ObjectValue != tempobj1.ObjectValue)
                            ctx.InstructionPointer = (int)BitConverter.ToInt64(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)]);
                        else
                            ctx.InstructionPointer += 5;
                        break;
                    }
                case 0x1B:
                    { // brbt
                        tempobj0 = ctx.FunctionStack.Peek();
                        tempobj1 = ctx.FunctionStack.Peek();
                        if ((long)tempobj0.ObjectValue > (long)tempobj1.ObjectValue)
                            ctx.InstructionPointer = (int)BitConverter.ToInt64(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)]);
                        else
                            ctx.InstructionPointer += 5;
                        break;
                    }
                case 0x1C:
                    { // brlt
                        tempobj0 = ctx.FunctionStack.Peek();
                        tempobj1 = ctx.FunctionStack.Peek();
                        if ((long)tempobj0.ObjectValue < (long)tempobj1.ObjectValue)
                            ctx.InstructionPointer = (int)BitConverter.ToInt64(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)]);
                        else
                            ctx.InstructionPointer += 5;
                        break;
                    }
                case 0x1D:
                    { // brez
                        tempobj0 = ctx.FunctionStack.Peek();
                        tempobj1 = ctx.FunctionStack.Peek();
                        if ((long)tempobj0.ObjectValue == 0)
                            ctx.InstructionPointer = (int)BitConverter.ToInt64(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)]);
                        else
                            ctx.InstructionPointer += 5;
                        break;
                    }
                case 0x1E:
                    { // brnez
                        tempobj0 = ctx.FunctionStack.Peek();
                        tempobj1 = ctx.FunctionStack.Peek();
                        if ((long)tempobj0.ObjectValue != 0)
                            ctx.InstructionPointer = (int)BitConverter.ToInt64(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)]);
                        else
                            ctx.InstructionPointer += 5;
                        break;
                    }
                case 0x1F:
                    { // dup
                        ctx.FunctionStack.Push(ctx.FunctionStack.Peek());
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x20:
                    { // pop
                        ctx.FunctionStack.Pop();
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x0C:
                    { // pusharg
                        var argin = ctx.FunctionStack.Pop();

                        if (argin.ObjectType != ExecutionEngine.ObjectTypeInteger)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to load argument, which index is not an integer"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);
                        var index = (long)argin.ObjectValue;

                        if (index < 0 || index > ctx.Function.RawInstructions.Length)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to load non-existent argument"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        ctx.FunctionStack.Push(ctx.Arguments[index]);

                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x22:
                    { // swap
                        tempobj0 = ctx.FunctionStack.Pop();
                        tempobj1 = ctx.FunctionStack.Pop();
                        ctx.FunctionStack.Push(tempobj0);
                        ctx.FunctionStack.Push(tempobj1);
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x23: // pushint.i
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeInteger)
                    {
                        ObjectValue = (long)BitConverter.ToInt32(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 5)])
                    });
                    ctx.InstructionPointer += 5;
                    break;
                case 0x24: // pushint.s
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeInteger)
                    {
                        ObjectValue = (long)BitConverter.ToInt16(ctx.Function.RawInstructions.AsSpan()[(ctx.InstructionPointer + 1)..(ctx.InstructionPointer + 3)])
                    });
                    ctx.InstructionPointer += 3;
                    break;
                case 0x25: // pushint.b
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeInteger)
                    {
                        ObjectValue = (long)(short)ctx.Function.RawInstructions.AsSpan()[ctx.InstructionPointer + 1]
                    });
                    ctx.InstructionPointer += 2;
                    break;
                case 0x26:
                    {   // setglobal
                        var ident = ctx.FunctionStack.Pop();
                        var value = ctx.FunctionStack.Pop();
                        if (ident.ObjectType != ExecutionEngine.ObjectTypeString)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to set value to global variable, which is not a string"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        value.References++;
                        if (ExecutionEngine.AllGlobalVariables.TryGetValue((string)ident.ObjectValue, out EEObject val))
                        {
                            val.References--;
                        }

                        ExecutionEngine.AllGlobalVariables[(string)ident.ObjectValue] = value;
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x27:
                    { // getglobal
                        var ident = ctx.FunctionStack.Pop();
                        if (ident.ObjectType != ExecutionEngine.ObjectTypeString)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to set value to global variable, which is not a string"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        ctx.FunctionStack.Push(ExecutionEngine.AllGlobalVariables[(string)ident.ObjectValue]);
                        ctx.InstructionPointer += 1;
                        break;
                    }
                case 0x28:
                    { // getlength
                        var obj = ctx.FunctionStack.Pop();
                        if (obj.ObjectType != ExecutionEngine.ObjectTypeCompound)
                            ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeString)
                            {
                                ObjectValue = "attempted to get length of value, which is not compound"
                            }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);

                        ctx.FunctionStack.Push(new(ExecutionEngine.ObjectTypeInteger) { ObjectValue = ((CustomDictionary)obj.ObjectValue).Count });
                        ctx.InstructionPointer += 1;
                        break;
                    }
                default:
                    ThrowAnException(new EEObject(ExecutionEngine.ObjectTypeCompound)
                    {
                        ObjectValue = new EEObject("unknown opcode")
                    }, ref ctx.FunctionResult, ref ctx.ExceptionHandlerStack, ref ctx.FunctionStack, ref ctx.IsRunning, ref ctx.InstructionPointer);
                    break;
            }
            if (dumpstack)
            {
                EEObject[] objs = ctx.FunctionStack.Reverse().ToArray();
                for (int j = 0; j < objs.Length; j++)
                    Console.WriteLine($"    Object #{j}: {objs[j]}");
            }
        }
    }
}
