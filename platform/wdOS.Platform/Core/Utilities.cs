using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;
using Cosmos.Core.Memory;
using XSharp.Assembler;
using XSharp;

namespace wdOS.Platform.Core
{
    public static unsafe class Utilities
    {
        public const string LowerCaseEnglishAlphabet = "abcdefghijklmnopqrstuvwxyz";
        public const string UpperCaseEnglishAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static void WaitFor(uint timeout)
        {
            var target = DateTime.Now.AddMilliseconds(timeout);
            while (DateTime.Now < target) { }
        }
        public static int FromOctal(byte* str, int size)
        {
            int n = 0;
            byte* c = str;
            while (size-- > 0)
            {
                n *= 8;
                n += *c - '0';
                c++;
            }
            return n;
        }
        public static string FromCString(char* c)
        {
            StringBuilder sb = new();
            byte* ptr = (byte*)c;
            for (int i = 0; ptr[i] != 0; i++)
                sb.Append((char)ptr[i]);
            return sb.ToString();
        }
        public static int StringLength(byte* c)
        {
            int length = 0;
            for (int i = 0; c[i] != 0; i++)
                length++;
            return length;
        }
        public static bool StringCheck(byte* str1, byte* str2, int length)
        {
            for (int i = 0; i < length; i++)
                if (str1[i] != str2[i])
                    return false;
            return true;
        }
        public static char* ToCString(string str)
        {
            char* c = (char*)Heap.Alloc((uint)(str.Length + 1));
            for (int i = 0; i < str.Length; i++) c[i] = str[i];
            c[str.Length] = '\0';
            return c;
        }
        public static string ConcatArray(string[] args) => ConnectArgs(args, ' ');
        public static string ConnectArgs(string[] args, char sep) => string.Join(sep, args);
        public static bool HasFlag(int value, int match) => (value & match) != 0;
    }

    public class CircularBuffer<T>
    {
        private int tail = 0;
        private int head = 0;
        private T[] buffer;
        public CircularBuffer(int capacity) => buffer = new T[capacity];
        public T Read() => buffer[Wrap(tail++)];
        public T Peek() => buffer[Wrap(tail)];
        public void Write(T value) => buffer[Wrap(head++)] = value;
        public void Clear() => tail = head;
        private int Wrap(int n) => n % buffer.Length;
    }
}
