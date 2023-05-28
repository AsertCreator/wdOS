using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wdOS.Pillow.ExecutionEngine;

namespace wdOS.Pillow
{
    public sealed class EEObject
    {
        public int ObjectType;
        public int References;
        public object ObjectValue;
        public EEObject(int objtype)
        {
            ObjectType = objtype;
            References = 0;
            ObjectValue = ObjectType switch
            {
                ObjectTypeInteger => 0L,
                ObjectTypeDecimal => 0.0,
                ObjectTypeBoolean => 0L, // false
                ObjectTypeString => "",
                ObjectTypeCompound => new CustomDictionary(),
                ObjectTypeFunction => new EEFunction(),
                ObjectTypeUndefined => null,
                ObjectTypeNull => null,
                _ => null,
            };
            AllObjects.Add(this);
        }
        public EEObject(string str)
        {
            ObjectType = ObjectTypeString;
            References = 0;
            ObjectValue = str;
            AllObjects.Add(this);
        }
        public static string DumpCompound(EEObject obj)
        {
            CustomDictionary dict = (CustomDictionary)obj.ObjectValue;
            StringBuilder sb = new();
            sb.Append("{ ");
            for (int i = 0; i < dict.Count; i++)
            {
                var key = dict.Keys[i];
                var val = dict.Values[i];
                sb.Append($"{key.ToString()}: {val.ToString()}");
                if (i != dict.Count - 1) sb.Append(", ");
            }
            sb.Append(" }");
            return sb.ToString();
        }
        public static EEObject operator +(EEObject b, EEObject a)
        {
            if (a.ObjectType == ObjectTypeInteger ||
                b.ObjectType == ObjectTypeInteger)
            {
                var obj = new EEObject(ObjectTypeInteger);
                obj.ObjectValue = (long)a.ObjectValue + (long)b.ObjectValue;
                return obj;
            }
            if (a.ObjectType == ObjectTypeDecimal ||
                b.ObjectType == ObjectTypeDecimal)
            {
                var obj = new EEObject(ObjectTypeDecimal);
                obj.ObjectValue = (double)a.ObjectValue + (double)b.ObjectValue;
                return obj;
            }
            if (a.ObjectType == ObjectTypeString ||
                b.ObjectType == ObjectTypeString)
            {
                var obj = new EEObject(ObjectTypeString);
                obj.ObjectValue = (string)a.ObjectValue + (string)b.ObjectValue;
                return obj;
            }
            return new EEObject(ObjectTypeNull);
        }
        public static EEObject operator -(EEObject b, EEObject a)
        {
            if (a.ObjectType == ObjectTypeInteger ||
                b.ObjectType == ObjectTypeInteger)
            {
                var obj = new EEObject(ObjectTypeInteger);
                obj.ObjectValue = (long)a.ObjectValue - (long)b.ObjectValue;
                return obj;
            }
            if (a.ObjectType == ObjectTypeDecimal ||
                b.ObjectType == ObjectTypeDecimal)
            {
                var obj = new EEObject(ObjectTypeDecimal);
                obj.ObjectValue = (double)a.ObjectValue - (double)b.ObjectValue;
                return obj;
            }
            return new EEObject(ObjectTypeNull);
        }
        public static EEObject operator *(EEObject b, EEObject a)
        {
            if (a.ObjectType == ObjectTypeInteger ||
                b.ObjectType == ObjectTypeInteger)
            {
                var obj = new EEObject(ObjectTypeInteger);
                obj.ObjectValue = (long)a.ObjectValue * (long)b.ObjectValue;
                return obj;
            }
            if (a.ObjectType == ObjectTypeDecimal ||
                b.ObjectType == ObjectTypeDecimal)
            {
                var obj = new EEObject(ObjectTypeDecimal);
                obj.ObjectValue = (double)a.ObjectValue * (double)b.ObjectValue;
                return obj;
            }
            return new EEObject(ObjectTypeNull);
        }
        public static EEObject operator /(EEObject b, EEObject a)
        {
            if (a.ObjectType == ObjectTypeInteger ||
                b.ObjectType == ObjectTypeInteger)
            {
                var obj = new EEObject(ObjectTypeInteger);
                obj.ObjectValue = (long)a.ObjectValue / (long)b.ObjectValue;
                return obj;
            }
            if (a.ObjectType == ObjectTypeDecimal ||
                b.ObjectType == ObjectTypeDecimal)
            {
                var obj = new EEObject(ObjectTypeDecimal);
                obj.ObjectValue = (double)a.ObjectValue / (double)b.ObjectValue;
                return obj;
            }
            return new EEObject(ObjectTypeNull);
        }
        public override string ToString() => ObjectType switch
        {
            ObjectTypeInteger => $"{ObjectValue}",
            ObjectTypeDecimal => $"{ObjectValue}",
            ObjectTypeBoolean => $"{ObjectValue}".ToLower(),
            ObjectTypeString => (string)ObjectValue,
            ObjectTypeCompound => DumpCompound(this),
            ObjectTypeFunction =>
            $"function () {{ " +
            $"{(((EEFunction)ObjectValue).Attribute == EEFunctionAttribute.Instrinsic ? "[native code]" : "...")}" +
            $" }}",
            ObjectTypeUndefined => "undefined",
            ObjectTypeNull => "null",
            _ => "unknown",
        };
    }
}
