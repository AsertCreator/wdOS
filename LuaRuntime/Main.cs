using AluminumLua;
using System;
using System.Linq;

namespace LuaRuntime
{

    public static class MainClass
    {

        public static void Main(string[] args)
        {
            LuaParser parser;

            LuaContext context = new();
            context.AddBasicLibrary();
            context.AddIoLibrary();

            if (args.Any())
            { // take first arg as file name
                parser = new LuaParser(context, args[0]);
                parser.Parse();

                return;
            }

            // otherwise, run repl
            Console.WriteLine("AluminumLua 0.1 (c) 2011 Alex Corrado");
            parser = new LuaParser(context);
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    parser.Parse(true);
                }
                catch (LuaException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

    }


}

