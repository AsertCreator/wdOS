using wdOS.Pillow;

namespace PillowTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EEExecutable exec = new();
            exec.AllStringLiterals.Add("Hello World!");
            exec.AllFunctions.Add(EEAssembler.AssemblePillowIL(
                ".maxlocal 1\n" +
                "pushobj\n" +
                "pushint.b 0\n" +
                "setlocal\n" +
                "pushint.b 0\n" +
                "getlocal\n" +
                "pushint.b 0\n" +
                "pushint.b 5\n" +
                "setfield\n" +
                "pushstr 0\n" +
                "pushint.i 5345446\n" +
                "setfield\n" +
                "ret"));
            exec.Entrypoint = exec.AllFunctions[0];
            ExecutionEngine.AllFunctions.Add(exec.Entrypoint);

            byte[] bytes = ExecutionEngine.Save(exec);
            File.WriteAllBytes("pillow.plex", bytes);

            var res = exec.Execute("");
            Console.WriteLine(res.ReturnedValue.ToString());
        }
    }
}