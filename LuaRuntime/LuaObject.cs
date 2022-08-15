using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LuaTable = System.Collections.Generic.IDictionary<LuaRuntime.LuaObject, LuaRuntime.LuaObject>;
using LuaTableImpl = System.Collections.Generic.Dictionary<LuaRuntime.LuaObject, LuaRuntime.LuaObject>;
using LuaTableItem = System.Collections.Generic.KeyValuePair<LuaRuntime.LuaObject, LuaRuntime.LuaObject>;

namespace LuaRuntime
{
    public delegate LuaObject LuaFunction(LuaObject[] args);
    public enum LuaType
    {
        nil, boolean, number, @string,
        userdata, function, thread, table
    };
    public struct LuaObject : IEnumerable<LuaTableItem>, IEquatable<LuaObject>
    {
        private object luaobj;
        public static readonly LuaObject Nil = new();
        public static readonly LuaObject True = new() { luaobj = true, Type = LuaType.boolean };
        public static readonly LuaObject False = new() { luaobj = false, Type = LuaType.boolean };
        public static readonly LuaObject Zero = new() { luaobj = 0d, Type = LuaType.number };
        public static readonly LuaObject EmptyString = new() { luaobj = "", Type = LuaType.@string };
        public LuaType Type { get; private set; }
        public bool Is(LuaType type) => Type == type;
        public static LuaObject FromBool(bool bln) => bln ? True : False;
        public static implicit operator LuaObject(bool bln) => FromBool(bln);
        public static LuaObject FromDelegate(Delegate a) => FromFunction((args) => DelegateAdapter(a, args));
        public static LuaObject FromObject(object obj)
        {
            switch (obj)
            {
                case null: return Nil;
                case LuaObject: return (LuaObject)obj;
                case bool: return FromBool((bool)obj);
            }
            if (obj is string str) _ = FromString(str);
            if (obj is LuaFunction @delegate) return FromFunction(@delegate);
            if (obj is Delegate @delegate) return FromDelegate(@delegate);
            if (obj is LuaTable dictionary) return FromTable(dictionary);
            return obj switch
            {
                double => FromNumber((double)obj),
                float => FromNumber((float)obj),
                int => FromNumber((int)obj),
                uint => FromNumber((uint)obj),
                short => FromNumber((short)obj),
                ushort => FromNumber((ushort)obj),
                long => FromNumber((long)obj),
                ulong => FromNumber((ulong)obj),
                byte => FromNumber((byte)obj),
                sbyte => FromNumber((sbyte)obj),
                Thread => new LuaObject { luaobj = obj, Type = LuaType.thread },
                _ => FromUserData(obj)
            };
        }
        private static LuaObject DelegateAdapter(Delegate @delegate, IEnumerable<LuaObject> args) =>
            FromObject(@delegate.DynamicInvoke((from a in args select a.luaobj).ToArray()));
        public static LuaObject FromNumber(double number)
        {
            return number == 0d ? Zero : new LuaObject { luaobj = number, Type = LuaType.number };
        }
        public static implicit operator LuaObject(double number) => FromNumber(number);
        public static LuaObject FromString(string str)
        {
            return str == null ? Nil : str.Length == 0 ? EmptyString : new LuaObject { luaobj = str, Type = LuaType.@string };
        }
        public static implicit operator LuaObject(string str)
        {
            return FromString(str);
        }

        public static LuaObject FromTable(LuaTable table)
        {
            return table == null ? Nil : new LuaObject { luaobj = table, Type = LuaType.table };
        }
        public static LuaObject FromUserData(object userdata)
        {
            return userdata == null ? Nil : new LuaObject { luaobj = userdata, Type = LuaType.userdata };
        }
        public static LuaObject NewTable(params LuaTableItem[] initItems)
        {
            LuaObject table = FromTable(new LuaTableImpl());

            foreach (LuaTableItem item in initItems)
            {
                table[item.Key] = item.Value;
            }

            return table;
        }

        public static LuaObject FromFunction(LuaFunction fn)
        {
            return fn == null ? Nil : new LuaObject { luaobj = fn, Type = LuaType.function };
        }
        public static implicit operator LuaObject(LuaFunction fn)
        {
            return FromFunction(fn);
        }

        public bool IsNil => Type == LuaType.nil;

        public bool IsBool => Type == LuaType.boolean;
        public bool AsBool()
        {
            return luaobj != null && (luaobj is not bool || (bool)luaobj != false);
        }

        public bool IsNumber => Type == LuaType.number;
        public double AsNumber()
        {
            return (double)luaobj;
        }
        public bool IsUserData => Type == LuaType.userdata;
        public object AsUserData()
        {
            return luaobj;
        }
        public bool IsString => Type == LuaType.@string;
        public string AsString()
        {
            return luaobj.ToString();
        }

        public bool IsFunction => Type == LuaType.function;
        public LuaFunction AsFunction()
        {
            return luaobj is not LuaFunction fn ? throw new LuaException("cannot call non-function") : fn;
        }

        public bool IsTable => Type == LuaType.table;
        public LuaTable AsTable()
        {
            return luaobj as LuaTable;
        }


        public IEnumerator<LuaTableItem> GetEnumerator()
        {
            return luaobj is not IEnumerable<LuaTableItem> table ? null : table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public LuaObject this[LuaObject key]
        {
            get
            {
                LuaTable table = AsTable();
                if (table == null)
                {
                    throw new LuaException("cannot index non-table");
                }

                _ = table.TryGetValue(key, out LuaObject result);
                return result;
            }
            set
            {
                LuaTable table = AsTable();
                if (table == null)
                {
                    throw new LuaException("cannot index non-table");
                }

                table[key] = value;
            }
        }
        public override string ToString()
        {
            if (IsNil)
            {
                return "nil";
            }

            return IsTable
                ? "{ " + string.Join(", ", AsTable().Select(kv => string.Format("[{0}]={1}", kv.Key, kv.Value.ToString())).ToArray()) + " }"
                : IsFunction ? AsFunction().Method.ToString() : luaobj.ToString();
        }
        public bool Equals(LuaObject other)
        {
            return other.Type == Type && (luaobj == null || luaobj.Equals(other.luaobj));
        }

        public override bool Equals(object obj)
        {
            return obj is LuaObject && Equals((LuaObject)obj);
        }
        public override int GetHashCode() { unchecked { return (luaobj != null ? luaobj.GetHashCode() : 0) ^ Type.GetHashCode(); } }
    }
}

