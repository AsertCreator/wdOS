/*
	InterpreterExecutor.cs: Immediately executes the code by interpreting it
	
	Copyright (c) 2011 Alexander Corrado
  
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
 */

using AluminumLua;
using System;
using System.Collections.Generic;

namespace LuaRuntime.Executors
{

    public class InterpreterExecutor : IExecutor
    {

        protected Stack<LuaContext> scopes;
        protected Stack<LuaObject> stack;

        public LuaContext CurrentScope => scopes.Peek();

        public InterpreterExecutor(LuaContext ctx)
        {
            scopes = new Stack<LuaContext>();
            scopes.Push(ctx);

            stack = new Stack<LuaObject>();
        }

        public virtual void PushScope()
        {
            scopes.Push(new LuaContext(CurrentScope));
        }

        public virtual void PushBlockScope()
        {
            throw new NotSupportedException();
        }

        public virtual void PushFunctionScope(string[] argNames)
        {
            throw new NotSupportedException();
        }

        public virtual void PopScope()
        {
            _ = scopes.Pop();
        }

        public virtual void Constant(LuaObject value)
        {
            stack.Push(value);
        }

        public virtual void Variable(string identifier)
        {
            stack.Push(CurrentScope.Get(identifier));
        }

        public virtual void Call(int argCount)
        {
            LuaObject[] args = new LuaObject[argCount];

            for (int i = argCount - 1; i >= 0; i--)
            {
                args[i] = stack.Pop();
            }

            stack.Push(stack.Pop().AsFunction()(args));
        }

        public virtual void TableCreate(int initCount)
        {
            LuaObject table = LuaObject.NewTable();

            for (int i = 0; i < initCount; i++)
            {
                LuaObject value = stack.Pop();
                LuaObject key = stack.Pop();

                table[key] = value;
            }

            stack.Push(table);
        }

        public virtual void TableGet()
        {
            LuaObject key = stack.Pop();
            LuaObject table = stack.Pop();
            stack.Push(table[key]);
        }

        public virtual void Concatenate()
        {
            string val2 = stack.Pop().AsString();
            string val1 = stack.Pop().AsString();

            stack.Push(LuaObject.FromString(string.Concat(val1, val2)));
        }

        public virtual void Negate()
        {
            bool val = stack.Pop().AsBool();
            stack.Push(LuaObject.FromBool(!val));
        }

        public virtual void Or()
        {
            bool val2 = stack.Pop().AsBool();
            bool val1 = stack.Pop().AsBool();
            stack.Push(LuaObject.FromBool(val1 || val2));
        }

        public virtual void And()
        {
            bool val2 = stack.Pop().AsBool();
            bool val1 = stack.Pop().AsBool();
            stack.Push(LuaObject.FromBool(val1 && val2));
        }

        public virtual void Equal()
        {
            LuaObject val2 = stack.Pop();
            LuaObject val1 = stack.Pop();
            stack.Push(LuaObject.FromBool(val1.Equals(val2)));
        }

        public virtual void NotEqual()
        {
            LuaObject val2 = stack.Pop();
            LuaObject val1 = stack.Pop();
            stack.Push(LuaObject.FromBool(!val1.Equals(val2)));
        }

        public virtual void IfThenElse()
        {
            LuaFunction Else = stack.Pop().AsFunction();
            LuaFunction Then = stack.Pop().AsFunction();
            bool Cond = stack.Pop().AsBool();
            _ = Cond ? Then.Invoke(new LuaObject[] { }) : Else.Invoke(new LuaObject[] { });
        }

        public virtual void Greater()
        {
            double val2 = stack.Pop().AsNumber();
            double val1 = stack.Pop().AsNumber();

            stack.Push(LuaObject.FromBool(val1 > val2));
        }
        public virtual void Smaller()
        {
            double val2 = stack.Pop().AsNumber();
            double val1 = stack.Pop().AsNumber();

            stack.Push(LuaObject.FromBool(val1 < val2));
        }
        public virtual void GreaterOrEqual()
        {
            double val2 = stack.Pop().AsNumber();
            double val1 = stack.Pop().AsNumber();

            stack.Push(LuaObject.FromBool(val1 >= val2));
        }
        public virtual void SmallerOrEqual()
        {
            double val2 = stack.Pop().AsNumber();
            double val1 = stack.Pop().AsNumber();

            stack.Push(LuaObject.FromBool(val1 <= val2));
        }

        public virtual void Add()
        {
            double val2 = stack.Pop().AsNumber();
            double val1 = stack.Pop().AsNumber();

            stack.Push(LuaObject.FromNumber(val1 + val2));
        }

        public virtual void Subtract()
        {
            double val2 = stack.Pop().AsNumber();
            double val1 = stack.Pop().AsNumber();

            stack.Push(LuaObject.FromNumber(val1 - val2));
        }

        public virtual void Multiply()
        {
            double val2 = stack.Pop().AsNumber();
            double val1 = stack.Pop().AsNumber();

            stack.Push(LuaObject.FromNumber(val1 * val2));
        }

        public virtual void Divide()
        {
            double val2 = stack.Pop().AsNumber();
            double val1 = stack.Pop().AsNumber();

            stack.Push(LuaObject.FromNumber(val1 / val2));
        }

        public virtual void PopStack()
        {
            _ = stack.Pop();
        }

        public virtual void Assign(string identifier, bool localScope)
        {
            if (localScope)
            {
                CurrentScope.SetLocal(identifier, stack.Pop());
            }
            else
            {
                CurrentScope.SetGlobal(identifier, stack.Pop());
            }
        }

        public virtual void TableSet()
        {
            LuaObject value = stack.Pop();
            LuaObject key = stack.Pop();
            LuaObject table = stack.Pop();

            table[key] = value;
        }

        public virtual void Return()
        {
            // FIXME: This will do something once the interpreter can support uncompiled functions /:
        }

        public virtual LuaObject Result()
        {
            return stack.Count > 0 ? stack.Pop() : LuaObject.Nil;
        }

        public void ColonOperator()
        {
            LuaObject key = stack.Pop();
            LuaObject table = stack.Pop();
            stack.Push(table[key]);
            stack.Push(table);
        }

    }
}

