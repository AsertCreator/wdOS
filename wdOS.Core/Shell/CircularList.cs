using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.Tokens;

namespace wdOS.Core.Shell
{
    internal class CircularList<T>
    {
        internal T[] ListEntries;
        internal int Index;
        internal int Capacity;
        internal CircularList(int capacity)
        {
            ListEntries = new T[capacity];
            Capacity = capacity;
            Index = 0;
        }
        internal void Add(T entry)
        {
            Index++;
            Index %= Capacity;
            ListEntries[Index] = entry;
        }
        internal T Pop()
        {
            var data = ListEntries[Index--];
            if (Index < 0)
            {
                Index = Capacity - 1;
            }
            return data;
        }
    }
}
