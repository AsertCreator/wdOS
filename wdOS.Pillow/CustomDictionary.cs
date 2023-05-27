using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Pillow
{
    public class CustomDictionary
    {
        public List<EEObject> Keys = new List<EEObject>();
        public List<EEObject> Values = new List<EEObject>();
        public int Count => Keys.Count;
        public EEObject GetValue(EEObject key) => Values[Keys.IndexOf(key)];
        public void SetKeyValue(EEObject key, EEObject value) => AddKeyValue(key, value);
        public void AddKeyValue(EEObject key, EEObject value)
        {
            if (Keys.Contains(key)) { Values[Keys.IndexOf(key)] = value; return; }
            Keys.Add(key);
            Values.Add(value);
        }
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[Count];
            for (int i = 0; i < Count; i++)
            {
                int val = (int)Values[i].ObjectValue;
                bytes[i] = (byte)val;
            }
            return bytes;
        }
    }
}
