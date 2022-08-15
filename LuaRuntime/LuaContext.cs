using LuaRuntime;
using System.Collections.Generic;

namespace AluminumLua
{

    public partial class LuaContext
    {
        // (see libraries)

        private IDictionary<string, LuaObject> variables;
        public IDictionary<string, LuaObject> Variables
        {
            get
            {
                if (variables == null)
                {
                    variables = new Dictionary<string, LuaObject>();
                }

                return variables;
            }
        }

        protected LuaContext Parent { get; private set; }

        public LuaContext(LuaContext parent)
        {
            Parent = parent;

            if (Parent.variables != null)
            {
                variables = new LinkedDictionary<string, LuaObject>(parent.variables);
            }
        }

        public LuaContext()
        {
            SetGlobal("true", LuaObject.True);
            SetGlobal("false", LuaObject.False);
            SetGlobal("nil", LuaObject.Nil);
        }

        public LuaObject Get(string name)
        {
            _ = Variables.TryGetValue(name, out LuaObject val);
            return val;
        }

        public bool IsDefined(string name)
        {
            return Variables.ContainsKey(name);
        }

        public void Define(string name)
        {
            if (!IsDefined(name))
            {
                Variables.Add(name, LuaObject.Nil);
            }
        }

        public void SetLocal(string name, LuaObject val)
        {
            if (Variables.ContainsKey(name))
            {
                _ = Variables.Remove(name);
            }

            if (!val.IsNil)
            {
                Variables.Add(name, val);
            }
        }
        public void SetLocal(string name, LuaFunction fn)
        {
            SetLocal(name, LuaObject.FromFunction(fn));
        }

        public void SetGlobal(string name, LuaObject val)
        {
            SetLocal(name, val);
            if (Parent != null)
            {
                Parent.SetGlobal(name, val);
            }
        }
        public void SetGlobal(string name, LuaFunction fn)
        {
            SetGlobal(name, LuaObject.FromFunction(fn));
        }


        public void SetLocalAndParent(string name, LuaObject val)
        {
            SetLocal(name, val);
            Parent.SetLocal(name, val);
        }
        public void SetLocalAndParent(string name, LuaFunction fn)
        {
            SetLocalAndParent(name, LuaObject.FromFunction(fn));
        }
    }
}

