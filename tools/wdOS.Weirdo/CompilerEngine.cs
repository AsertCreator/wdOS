using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using System.Text;
using Antlr4.Runtime.Misc;
using wdOS.Pillow;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Linq;

namespace wdOS.Weirdo
{
    public static class CompilerEngine
    {
        private static List<string> GlobalVariableContexts = new();
        private static List<FunctionContext> FunctionContexts = new();
        public static List<EEFunction> CompiledFunctions = new();
        public static List<string> StringLiterals = new();
        public static string? Entrypoint = "main";
        public static bool ErrorOccurred = false;

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
                var comp = EEAssembler.AssemblePillowIL(CompileFunction(fctx));
                CompiledFunctions.Add(comp);
                fctx.FinalFunction = comp;
            }

            CompiledFunctions.Add(new EEFunction()
            {
                LocalCount = 0, ArgumentCount = 1,
                Attribute = EEFunctionAttribute.Instrinsic, AttributeAux = 1
            });

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
                .FirstOrDefault()!;

            if (exec.Entrypoint == default)
            {
                ThrowAnError(0, "entrypoint set to non-existent function!");
            }

            exec.AllStringLiterals.AddRange(StringLiterals);
            exec.AllFunctions.AddRange(CompiledFunctions);

            return exec;
        }
        internal static string CompileFunction(FunctionContext ctx)
        {
            StringBuilder sb = new();
            Dictionary<string, int> locals = new();
            bool hasreturned = false;

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

                token = context.GetChild<WeirdoGrammarParser.String_literalContext>(0);
                if (token != null)
                {
                    sb.AppendLine($"pushstr {StringLiterals.IndexOf(token.GetText()[1..^1])}");
                    return;
                }

                token = context.GetToken(WeirdoGrammarLexer.ID, 0);
                if (token != null)
                {
                    string id = token.GetText();
                    EmitIntPushing(GetLocalID(id, ((CommonToken)((TerminalNodeImpl)token).Payload).Line));
                    sb.AppendLine("getlocal");
                    return;
                }
            }
            int GetLocalID(string local, int line)
            {
                if (!locals.ContainsKey(local))
                {
                    ThrowAnError(line, $"what is \"{local}\"? i don't know that, move that local's declaration upwards");
                    return 0;
                }
                return locals.Keys.ToList().IndexOf(local);
            }

            for (int i = 0; i < ctx.Statements.Length; i++)
            {
                var statement = ctx.Statements[i];
                var token = statement.GetChild(0);

                if (statement.LOCAL() != null)
                {
                    var name = statement.GetChild(1).GetText();

                    if (locals.ContainsKey(name))
                        ThrowAnError(statement.Start.Line, $"redefinition of local \"{name}\"");

                    else
                    {
                        if (statement.ChildCount == 2) // uninitalized local
                        {
                            locals.Add(name, i);
                            sb.AppendLine("pushundf");
                            EmitIntPushing(i);
                            sb.AppendLine("setlocal");
                        }

                        else if (statement.ChildCount == 4) // initialized local
                        {
                            locals.Add(name, i);
                            EmitExpressionEvaluation((WeirdoGrammarParser.ExpressionContext)statement.GetChild(3));
                            EmitIntPushing(i);
                            sb.AppendLine("setlocal");
                        }
                    }
                }
                else if (statement.RETURN() != null)
                {
                    EmitExpressionEvaluation(
                        (WeirdoGrammarParser.ExpressionContext)statement.GetChild(1));
                    sb.AppendLine("ret");
                    hasreturned = true;
                }
                else if (statement.push_expression() != null)
                {
                    EmitExpressionEvaluation(
                        (WeirdoGrammarParser.ExpressionContext)statement.push_expression().GetChild(2));
                }
                else if (statement.popl_expression() != null)
                {
                    EmitIntPushing(GetLocalID(statement.popl_expression().GetChild(2).GetText(), ((TerminalNodeImpl)token.Payload).Symbol.Line));
                    sb.AppendLine("setlocal");
                }
                else // assume we're setting certain value to local
                {
                    var stoken = statement.GetChild(1);
                    var sctoken = (CommonToken)stoken.Payload;

                    if (sctoken.Type == WeirdoGrammarLexer.EQ) // setting certain value to local itself
                    {
                        EmitExpressionEvaluation(
                            (WeirdoGrammarParser.ExpressionContext)statement.GetChild(2));
                        EmitIntPushing(GetLocalID(token.GetText(), ((CommonToken)token.Payload).Line));
                        sb.AppendLine("setlocal");
                    }
                    else if (sctoken.Type == WeirdoGrammarLexer.DOT) // setting certain value to local's field
                    {
                        var ttoken = statement.GetChild(2);
                        var tctoken = (CommonToken)ttoken.Payload;

                        if (tctoken.Type == WeirdoGrammarLexer.ID) // are we?
                        {
                            EmitExpressionEvaluation(
                                (WeirdoGrammarParser.ExpressionContext)statement.GetChild(2));
                            EmitIntPushing(GetLocalID(token.GetText(), ((CommonToken)token.Payload).Line));
                            sb.AppendLine("setlocal");
                        }
                        else
                        {
                            ThrowAnError(tctoken.Line, "wait, that's not a valid name for a field!");
                        }
                    }
                    else // assume we're not
                    {
                        ThrowAnError(sctoken.Line, "i wonder what you are trying to do there");
                    }
                }
            }

            if (!hasreturned)
            {
                sb.AppendLine("pushundf");
                sb.AppendLine("ret");
            }

            sb.Insert(0, $".maxarg {ctx.Arguments.Length}\n");
            sb.Insert(0, $".maxlocal {locals.Count}\n");

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
                string str = context.GetText()[1..^1];
                if (!StringLiterals.Contains(str))
                    StringLiterals.Add(str);
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
