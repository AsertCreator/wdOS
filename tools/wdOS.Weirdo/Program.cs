using System;
using System.IO;
using wdOS.Pillow;
using wdOS.Platform;

namespace wdOS.Weirdo
{
    internal static class Program
    {
        internal static string? InputFilePath;
        internal static string? OutputFilePath = "out.plex";
        internal static bool RunAfterCompilation = false;
        internal static bool DebugMode = false;
        internal static int Main(string[] args)
        {
            if (!ParseConsoleArguments(args)) return 1;
            if (!File.Exists(InputFilePath))
            {
                Console.WriteLine($"weirdoc: specified file doesn't exist!");
                if (DebugMode) Console.WriteLine(InputFilePath);
                return 1;
            }
            string content = File.ReadAllText(InputFilePath);
            EEExecutable output = CompilerEngine.CompileFile(content);

            if (!CompilerEngine.ErrorOccurred)
            {
                byte[] bytes = ExecutionEngine.Save(output);
                File.WriteAllBytes(OutputFilePath ??
                    throw new NullReferenceException("s" +
                    "omehow output file path became null"), bytes);
                
                Console.WriteLine($"written {bytes.Length} bytes");

                if (RunAfterCompilation)
                {
                    Console.WriteLine("starting program...");

                    RuntimeManager.Initialize();

                    var res = output.Execute("");
                    if (res.IsExceptionUnwinding)
                        Console.WriteLine($"exception occurred! \"{res.ExceptionObject}\"");
                    else
                        Console.WriteLine(res.ReturnedValue);
                }

                return 0;
            }
            return 2;
        }
        internal static bool ParseConsoleArguments(string[] args)
        {
            bool foundinput = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--input" or "-i":
                        foundinput = true;
                        InputFilePath = args[++i];
                        break;
                    case "--output" or "-o":
                        OutputFilePath = args[++i];
                        break;
                    case "--debug-mode" or "-dbg":
                        DebugMode = true;
                        break;
                    case "--run" or "-r":
                        RunAfterCompilation = true;
                        break;
                    case "--help" or "-h":
                        Console.WriteLine("usage: weirdoc <arguments>\n");
                        Console.WriteLine("    --input|-i           sets input source file");
                        Console.WriteLine("    --output|-o          sets output PLEX file");
                        Console.WriteLine("    --debug-mode|-dbg    enabled debug mode");
                        Console.WriteLine("    --run|-r             if enabled compiler will start you program after compilation");
                        Console.WriteLine("    --help|-h            shows help menu\n");
                        Console.WriteLine("    --version|-v         shows version information\n");
                        Environment.Exit(0);
                        break;
                    case "--version" or "-v":
                        Console.WriteLine("weirdoc - version 0.1.0");
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine($"weirdoc: unknown console argument {args[i]}");
                        return false;
                }
            }
            if (!foundinput)
            {
                Console.WriteLine($"weirdoc: no input file specified");
                return false;
            }
            return true;
        }
    }
}