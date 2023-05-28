using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using System.Text;
using Antlr4.Runtime.Misc;
using wdOS.Pillow;

namespace wdOS.Weirdo
{
    public static class Compiler
    {
        public static List<EEFunction> CompiledFunctions = new();
        public static string? Entrypoint = "main";
        public static List<string> StringLiterals = new();
        public static bool ErrorOccurred = false;
        private static List<string> GlobalVariableContexts = new();
        private static List<FunctionContext> FunctionContexts = new();
        public static void ThrowAnError(int i, string message)
        {
            Console.Error.WriteLine($"weirdoc: error \"{message}\" at line {i}");
            ErrorOccurred = true;
        }
        public static void ThrowAnWarning(int i, string message) =>
            Console.Error.WriteLine($"weirdoc: warning \"{message}\" at line {i}");
        public static EEExecutable CompileFile(string content)
        {
            EEExecutable exec = new();

            var lexer = new WeirdoGrammarLexer(CharStreams.fromString(content));
            var tokens = new CommonTokenStream(lexer);
            var parser = new WeirdoGrammarParser(tokens)
            {
                BuildParseTree = true
            };
            var tree = parser.program();

            var context = new WeirdoCompilerListener();
            ParseTreeWalker.Default.Walk(context, tree);

            foreach (var fctx in FunctionContexts)
            {
                var comp = EEAssembler.AssemblePillowIL(CompileFunction(fctx, context));
                CompiledFunctions.Add(comp);
                fctx.FinalFunction = comp;
            }

            if (Program.DebugMode)
            {
                foreach (var item in StringLiterals)
                    Console.WriteLine($"- string: \"{item}\"");

                foreach (var item in GlobalVariableContexts)
                    Console.WriteLine($"- global variable: \"{item}\"");

                foreach (var item in FunctionContexts)
                {
                    Console.WriteLine($"- function: \"{item.Name}\", args: {string.Join(',', item.Arguments)}");
                    Console.WriteLine(item.FinalAssembly);
                }
            }
            exec.Entrypoint =
                (from x in FunctionContexts where x.Name == Entrypoint select x.FinalFunction)
                .FirstOrDefault();

            if (exec.Entrypoint == default)
            {
                ThrowAnError(0, "entrypoint set to non-existent function!");
            }

            exec.AllStringLiterals.AddRange(StringLiterals);
            exec.AllFunctions.AddRange(CompiledFunctions);

            return exec;
        }
        private static string CompileFunction(FunctionContext ctx, WeirdoCompilerListener cctx)
        {
            StringBuilder sb = new();
            Dictionary<string, (int, WeirdoGrammarParser.ExpressionContext?)> locals = new();
            bool hasreturned = false;

            SetupLocals();

            sb.AppendLine($".maxarg {ctx.Arguments.Length}");
            sb.AppendLine($".maxlocal {locals.Count}");

            EmitLocalInitialization();

            void EmitIntPushing(long num)
            {
                if (num >= 0)
                {
                    if (num <= byte.MaxValue) sb.AppendLine($"pushint.b {num}");
                    else if (num <= short.MaxValue) sb.AppendLine($"pushint.s {num}");
                    else if (num <= int.MaxValue) sb.AppendLine($"pushint.i {num}");
                    else sb.AppendLine($"pushint {num}");
                }
                else
                {
                    num = -num;
                    if (num <= short.MaxValue) sb.AppendLine($"pushint.s {-num}");
                    else if (num <= int.MaxValue) sb.AppendLine($"pushint.i {-num}");
                    else sb.AppendLine($"pushint {-num}");
                }
            }
            void EmitExpressionEvaluation(WeirdoGrammarParser.ExpressionContext context)
            {
                IParseTree token;

                token = context.GetToken(WeirdoGrammarLexer.INT, 0);
                if (token != null)
                {
                    EmitIntPushing(long.Parse(token.GetText()));
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.TRUE, 0);
                if (token != null)
                {
                    EmitIntPushing(1);
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.FALSE, 0);
                if (token != null)
                {
                    EmitIntPushing(0);
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.OBJECT, 0);
                if (token != null)
                {
                    sb.AppendLine("pushobj");
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.ADD, 0);
                if (token != null)
                {
                    EmitExpressionEvaluation((WeirdoGrammarParser.ExpressionContext)context.children[0]);
                    EmitExpressionEvaluation((WeirdoGrammarParser.ExpressionContext)context.children[2]);
                    sb.AppendLine("add");
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.SUB, 0);
                if (token != null)
                {
                    EmitExpressionEvaluation((WeirdoGrammarParser.ExpressionContext)context.children[0]);
                    EmitExpressionEvaluation((WeirdoGrammarParser.ExpressionContext)context.children[2]);
                    sb.AppendLine("sub");
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.MUL, 0);
                if (token != null)
                {
                    EmitExpressionEvaluation((WeirdoGrammarParser.ExpressionContext)context.children[0]);
                    EmitExpressionEvaluation((WeirdoGrammarParser.ExpressionContext)context.children[2]);
                    sb.AppendLine("mul");
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.DIV, 0);
                if (token != null)
                {
                    EmitExpressionEvaluation((WeirdoGrammarParser.ExpressionContext)context.children[0]);
                    EmitExpressionEvaluation((WeirdoGrammarParser.ExpressionContext)context.children[2]);
                    sb.AppendLine("div");
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.NULL, 0);
                if (token != null)
                {
                    sb.AppendLine("pushnull");
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.UNDEFINED, 0);
                if (token != null)
                {
                    sb.AppendLine("pushundf");
                    return;
                }

                token = context.GetChild<WeirdoGrammarParser.Function_call_expressionContext>(0);
                if (token != null)
                {
                    string name = token.GetChild(2).GetText();
                    FunctionContext? ctx = (from x in FunctionContexts where x.Name == name select x).FirstOrDefault();
                    if (ctx != default)
                    {
                        sb.AppendLine($"pushfunc {FunctionContexts.IndexOf(ctx)}");
                        sb.AppendLine("call");
                        return;
                    }
                    ThrowAnError(((ParserRuleContext)token).Start.Line, $"function \"{name}\" is not declared");
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.STRING_LITERAL, 0);
                if (token != null)
                {
                    sb.AppendLine($"pushstr {StringLiterals.IndexOf(token.GetText())}");
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.ID, 0);
                if (token != null)
                {
                    string id = token.GetText();
                    if (!locals.ContainsKey(id))
                    {
                        ThrowAnError(((ParserRuleContext)token).Start.Line, $"local \"{id}\" is not defined at this moment. consider moving it.");
                        return;
                    }
                    EmitIntPushing(locals[id].Item1);
                    sb.AppendLine("getlocal");
                    return;
                }
            }
            void EmitLocalInitialization()
            {
                for (int i = 0; i < locals.Count; i++)
                {
                    var kvp = locals.ElementAt(i);
                    if (kvp.Value.Item2 != null)
                    {
                        EmitExpressionEvaluation(kvp.Value.Item2);
                        SetLastStackObjectToLocal(kvp.Value.Item1);
                    }
                    else
                    {
                        sb.AppendLine("pushundf");
                        SetLastStackObjectToLocal(kvp.Value.Item1);
                    }
                }
            }
            void SetLastStackObjectToLocal(int i)
            {
                EmitIntPushing(i);
                sb.AppendLine("setlocal");
            }
            void SetupLocals()
            {
                for (int i = 0; i < ctx.Statements.Length; i++)
                {
                    var statement = ctx.Statements[i];
                    var local = statement.GetChild(0);
                    if (local.GetText() == "local")
                    {
                        if (statement.ChildCount == 2) locals.Add(statement.GetChild(1).GetText(), (i, null));
                        else if (statement.ChildCount == 4)
                            locals.Add(statement.GetChild(1).GetText(), (i,
                                (WeirdoGrammarParser.ExpressionContext)(ParserRuleContext)(RuleContext)statement.GetChild(3)));
                    }
                }
            }

            for (int i = 0; i < ctx.Statements.Length; i++)
            {
                var statement = ctx.Statements[i];
                var firstch = statement.GetChild(0);
                if (firstch.GetText() == "local") { }
                else if (firstch.GetText() == "return")
                {
                    EmitExpressionEvaluation(
                        (WeirdoGrammarParser.ExpressionContext)statement.GetChild(1));
                    sb.AppendLine("ret");
                    hasreturned = true;
                }
            }

            if (!hasreturned)
            {
                sb.AppendLine("pushundf");
                sb.AppendLine("ret");
            }

            ctx.FinalAssembly = sb;

            return sb.ToString();
        }
        internal class WeirdoCompilerListener : WeirdoGrammarBaseListener
        {
            public override void EnterEveryRule(ParserRuleContext ctx)
            {
                if (Program.DebugMode)
                    Console.WriteLine($"{new string(' ', (ctx.Depth() - 1) * 4)}- rule {WeirdoGrammarParser.ruleNames[ctx.RuleIndex]}, \"{ctx.GetText()}\"");
            }
            public override void EnterString_literal([NotNull] WeirdoGrammarParser.String_literalContext context)
            {
                StringLiterals.Add(context.GetText()[1..^1]);
            }
            public override void EnterGlobal_directive([NotNull] WeirdoGrammarParser.Global_directiveContext context)
            {
                GlobalVariableContexts.Add(context.children[1].GetText());
            }
            public override void EnterEntrypoint_directive([NotNull] WeirdoGrammarParser.Entrypoint_directiveContext context)
            {
                Entrypoint = context.children[2].GetText();
            }
            public override void EnterFunction([NotNull] WeirdoGrammarParser.FunctionContext context)
            {
                FunctionContext fctx = new()
                {
                    Name = context.function_name().GetText(),
                    Arguments = context.arg_list().GetText().Split(',', StringSplitOptions.RemoveEmptyEntries),
                    Statements = context.statement()
                };
                FunctionContexts.Add(fctx);
            }
            public override void VisitErrorNode(IErrorNode node)
            {
                ThrowAnError(node.Symbol.Line, "parsing/lexing error, double check your code");
            }
        }
        internal class FunctionContext
        {
            internal WeirdoGrammarParser.StatementContext[] Statements { get; set; } = Array.Empty<WeirdoGrammarParser.StatementContext>();
            internal StringBuilder? FinalAssembly { get; set; } = new();
            internal EEFunction? FinalFunction { get; set; } = null;
            internal string[] Arguments { get; set; } = Array.Empty<string>();
            internal string Name { get; set; } = "";
        }
    }
}
