/*
	LuaParser.cs: Handwritten Lua parser
	
	Copyright (c) 2011 Alexander Corrado
  
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
 */

// Please note: Work in progress.
// For instance, there is little support for Lua statement types beside function calls

using AluminumLua;
using LuaRuntime.Executors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LuaRuntime
{

    public class LuaParser : IDisposable
    {
        ~LuaParser()
        {
            Dispose(false);
        }
        protected string file_name;
        protected TextReader input;

        private readonly bool closeInputStream;

        protected bool eof, OptionKeyword;
        protected int row, col, scope_count;

        protected IExecutor CurrentExecutor { get; set; }

        public LuaParser(IExecutor executor, string file)
        {
            file_name = Path.GetFileName(file);
            input = File.OpenText(file);
            closeInputStream = true;
            row = 1;
            col = 1;

            CurrentExecutor = executor;
        }

        public LuaParser(LuaContext ctx, string file)
            : this(LuaSettings.Executor(ctx), file)
        {
        }

        public LuaParser(IExecutor executor)
        {
            file_name = "stdin";
#if WINDOWS_PHONE || SILVERLIGHT
            this.input = Console.In;
#else
            input = new StreamReader(Console.OpenStandardInput());
#endif
            CurrentExecutor = executor;
        }

        public LuaParser(LuaContext ctx, TextReader stream)
            : this(LuaSettings.Executor(ctx), stream)
        {
        }
        public LuaParser(IExecutor executor, TextReader stream)
        {
            file_name = "stream";
            input = stream;
            CurrentExecutor = executor;
        }

        public LuaParser(LuaContext ctx)
            : this(LuaSettings.Executor(ctx))
        {
        }

        public void Parse()
        {
            Parse(false);
        }

        public void Parse(bool interactive)
        {
            do
            {

                string identifier = ParseLVal();

                if (eof)
                {
                    break;
                }

                switch (identifier)
                {

                    case "function":
                        ParseFunctionDefStatement(false);
                        break;

                    case "do":
                        scope_count++;
                        CurrentExecutor.PushScope();
                        break;

                    case "else":
                    case "end":
                        if (--scope_count < 0)
                        {
                            scope_count = 0;
                            Err("unexpected 'end'");
                        }
                        OptionKeyword = identifier == "else";
                        CurrentExecutor.PopScope();
                        break;

                    case "local":
                        string name = ParseLVal();
                        if (name == "function")
                        {
                            ParseFunctionDefStatement(true);
                        }
                        else
                        {
                            ParseAssign(name, true);
                        }

                        break;

                    case "return":
                        ParseRVal();
                        CurrentExecutor.Return();
                        break;

                    case "if":
                        ParseConditionalStatement();
                        if (CurrentExecutor is CompilerExecutor)
                        {
                            CurrentExecutor.PopStack();
                        }

                        break;
                    default:
                        if (Peek() == '=')
                        {
                            ParseAssign(identifier, false);
                            break;
                        }

                        CurrentExecutor.Variable(identifier);
                        ParseLValOperator();
                        break;
                }

            } while (!eof && !interactive);
        }

        protected string ParseLVal()
        {
            string val = ParseOne(false);
            if (val == null && !eof)
            {
                Err("expected identifier");
            }

            return val;
        }

        protected void ParseLValOperator()
        {
            bool isTerminal = false;

            while (true)
            {

                switch (Peek())
                {

                    case '[':
                    case '.':
                        ParseTableAccess();
                        isTerminal = false;

                        // table assign is special
                        if (Peek() == '=')
                        {
                            _ = Consume();
                            ParseRVal();
                            CurrentExecutor.TableSet();
                            return;
                        }
                        break;
                    case ':':
                        ParseColonOperator();
                        isTerminal = true;
                        break;

                    case '(':
                    case '{':
                    case '"':
                    case '\'':
                        ParseCall(0);
                        isTerminal = true;
                        break;

                    default:
                        if (isTerminal)
                        {
                            CurrentExecutor.PopStack();
                            return;
                        }
                        Err("syntax error");
                        break;
                }
            }
        }

        protected void ParseRVal()
        {
            string identifier = ParseOne(true);

            if (identifier != null)
            {
                switch (identifier)
                {

                    case "function":
                        int currentScope = scope_count;
                        CurrentExecutor.PushFunctionScope(ParseArgDefList());
                        scope_count++;

                        while (scope_count > currentScope)
                        {
                            Parse(true);
                        }

                        break;

                    case "not":
                        ParseRVal();
                        CurrentExecutor.Negate();
                        break;

                    default:
                        CurrentExecutor.Variable(identifier);
                        break;
                }
            }

            while (TryParseOperator()) { /* do it */ }
        }

        protected bool TryParseOperator()
        {
            char next = Peek();

            switch (next)
            {

                case '[':
                    ParseTableAccess();
                    break;

                case '.':
                    ParseTableAccessOrConcatenation();
                    break;
                case ':':
                    ParseColonOperator();
                    break;
                case '(':
                case '{':
                case '"':
                case '\'':
                    ParseCall(0);
                    break;

                // FIXME: ORDER OF OPERATIONS!
                case '+':
                    _ = Consume();
                    ParseRVal();
                    CurrentExecutor.Add();
                    break;

                case '-':
                    _ = Consume();
                    ParseRVal();
                    CurrentExecutor.Subtract();
                    break;

                case '*':
                    _ = Consume();
                    ParseRVal();
                    CurrentExecutor.Multiply();
                    break;

                case '/':
                    _ = Consume();
                    ParseRVal();
                    CurrentExecutor.Divide();
                    break;

                case 'o':
                case 'O':
                    _ = Consume();
                    if (char.ToLowerInvariant(Consume()) != 'r')
                    {
                        Err("unexpected 'o'");
                    }

                    ParseRVal();
                    CurrentExecutor.Or();
                    break;

                case 'a':
                case 'A':
                    _ = Consume();
                    if (char.ToLowerInvariant(Consume()) != 'n')
                    {
                        Err("unexpected 'a'");
                    }

                    if (char.ToLowerInvariant(Consume()) != 'd')
                    {
                        Err("unexpected 'an'");
                    }

                    ParseRVal();
                    CurrentExecutor.And();
                    break;
                case '>':
                    _ = Consume();
                    bool OrEqual = Peek() == '=';
                    if (OrEqual)
                    {
                        _ = Consume();
                    }

                    ParseRVal();
                    if (OrEqual)
                    {
                        CurrentExecutor.GreaterOrEqual();
                    }
                    else
                    {
                        CurrentExecutor.Greater();
                    }

                    break;
                case '<':
                    _ = Consume();
                    bool OrEqual2 = Peek() == '=';
                    if (OrEqual2)
                    {
                        _ = Consume();
                    }

                    ParseRVal();
                    if (OrEqual2)
                    {
                        CurrentExecutor.SmallerOrEqual();
                    }
                    else
                    {
                        CurrentExecutor.Smaller();
                    }

                    break;
                case '=':
                    _ = Consume();
                    if (Consume() != '=')
                    {
                        Err("unexpected '='");
                    }

                    ParseRVal();
                    CurrentExecutor.Equal();
                    break;

                case '~':
                    _ = Consume();
                    if (Consume() != '=')
                    {
                        Err("unexpected '~'");
                    }

                    ParseRVal();
                    CurrentExecutor.NotEqual();
                    break;

                default:
                    return false;
            }

            return true;
        }

        protected void ParseAssign(string identifier, bool localScope)
        {
            Consume('=');
            ParseRVal(); // push value

            CurrentExecutor.Assign(identifier, localScope);
        }

        protected void ParseCall(int args)
        {
            int argCount = args;
            char next = Peek();

            if (next is '"' or '\'' or '{')
            {
                // function call with 1 arg only.. must be string or table

                ParseRVal();
                ++argCount;

            }
            else if (next == '(')
            {
                // function call

                _ = Consume();

                next = Peek();
                while (next != ')')
                {

                    ParseRVal();
                    ++argCount;

                    next = Peek();
                    if (next == ',')
                    {
                        _ = Consume();
                        next = Peek();

                    }
                    else if (next != ')')
                    {

                        Err("expecting ',' or ')'");
                    }
                }

                Consume(')');

            }
            else
            {

                Err("expecting string, table, or '('");
            }

            CurrentExecutor.Call(argCount);
        }

        protected void ParseConditionalStatement()
        {
            ParseRVal();
            if (char.ToLowerInvariant(Consume()) is not ('t' and
                'h' and
                'e' and
                'n'))
            {
                Err("Expected 'then'");
            }

            int currentScope = scope_count;
            CurrentExecutor.PushBlockScope();
            scope_count++;

            while (scope_count > currentScope && !eof)
            {
                Parse(true);
            }

            if (eof) { CurrentExecutor.PopScope(); OptionKeyword = false; }

            if (OptionKeyword)
            {
                currentScope = scope_count;
                CurrentExecutor.PushBlockScope();
                scope_count++;

                while (scope_count > currentScope && !eof)
                {
                    Parse(true);
                }

                if (eof)
                {
                    CurrentExecutor.PopScope();
                }
            }
            else
            {
                CurrentExecutor.PushBlockScope();
                CurrentExecutor.PopScope();
            }

            CurrentExecutor.IfThenElse();
        }

        protected void ParseFunctionDefStatement(bool localScope)
        {
            string name = ParseLVal();
            char next = Peek();
            bool isTableSet = next == '.';

            if (isTableSet)
            {

                CurrentExecutor.Variable(name); // push (first) table
                _ = Consume(); // '.'
                CurrentExecutor.Constant(ParseLVal()); // push key

                next = Peek();

                while (next == '.')
                {

                    CurrentExecutor.TableGet(); // push (subsequent) table
                    _ = Consume(); // '.'
                    CurrentExecutor.Constant(ParseLVal()); // push key

                    next = Peek();
                }
            }

            int currentScope = scope_count;
            CurrentExecutor.PushFunctionScope(ParseArgDefList());
            scope_count++;

            while (scope_count > currentScope)
            {
                Parse(true);
            }

            if (isTableSet)
            {
                CurrentExecutor.TableSet();
            }
            else
            {
                CurrentExecutor.Assign(name, localScope);
            }
        }

        // parses named argument list in func definition
        protected string[] ParseArgDefList()
        {
            Consume('(');

            List<string> args = new();
            char next = Peek();

            while (next != ')')
            {

                args.Add(ParseLVal());

                next = Peek();
                if (next == ',')
                {
                    _ = Consume();
                    next = Peek();

                }
                else if (next != ')')
                {

                    Err("expecting ',' or ')'");
                }
            }

            Consume(')');
            return args.ToArray();
        }

        protected void ParseColonOperator()
        {
            char next = Peek();

            if (next != ':')
            {
                Err("expected ':'");
            }

            _ = Consume();

            CurrentExecutor.Constant(ParseLVal()); // push key
            CurrentExecutor.ColonOperator();

            next = Peek();

            if (next != '(')
            {
                Err("expected '('");
            }

            ParseCall(1);

            //throw new NotImplementedException("colon operator is not implemented yet");
        }

        // assumes that the table has already been pushed
        protected void ParseTableAccess()
        {
            char next = Peek();

            if (next is not '[' and not '.')
            {
                Err("expected '[' or '.'");
            }

            _ = Consume();

            switch (next)
            {

                case '[':
                    ParseRVal(); // push key
                    Consume(']');
                    break;

                case '.':
                    CurrentExecutor.Constant(ParseLVal()); // push key
                    break;

            }

            if (Peek() != '=')
            {
                CurrentExecutor.TableGet();
            }
        }

        // assumes that the first item has already been pushed
        protected void ParseTableAccessOrConcatenation()
        {
            Consume('.');
            char next = Peek();

            if (next == '.')
            {
                // concatenation

                _ = Consume();
                ParseRVal();
                CurrentExecutor.Concatenate();

            }
            else
            {
                // table access

                CurrentExecutor.Constant(ParseLVal());
                CurrentExecutor.TableGet();

            }
        }

        // -----

        // Parses a single value or identifier
        // Identifiers come out as strings
        protected string ParseOne(bool expr)
        {
        top:
            char la = Peek();
            switch (la)
            {

                case (char)0: eof = true; break;

                case ';':
                    _ = Consume();
                    goto top;

                case '-':
                    _ = Consume();
                    if (Peek() == '-')
                    {
                        ParseComment();
                        goto top;

                    }
                    else if (expr)
                    {

                        ParseRVal();
                        CurrentExecutor.Constant(LuaObject.FromNumber(-1));
                        CurrentExecutor.Multiply();
                    }
                    break;

                case '"': if (expr) { CurrentExecutor.Constant(ParseStringLiteral()); } break;
                case '\'': if (expr) { CurrentExecutor.Constant(ParseStringLiteral()); } break;
                case '{': if (expr) { ParseTableLiteral(); } break;

                case '(':
                    if (expr)
                    {
                        _ = Consume();
                        ParseRVal();
                        Consume(')');
                    }
                    break;

                default:
                    if (char.IsLetter(la))
                    {
                        return ParseIdentifier();
                    }

                    if (expr && (char.IsDigit(la) || la == '.'))
                    {
                        CurrentExecutor.Constant(ParseNumberLiteral());
                    }
                    else
                    {
                        Err("unexpected '{0}'", la);
                    }

                    break;
            }

            return null; //?
        }

        // http://www.lua.org/pil/3.6.html
        protected void ParseTableLiteral()
        {
            Consume('{');

            int i = 1;
            int count = 0;
            char next = Peek();

            while (next != '}' && !eof)
            {
                count++;

                if (next == '[')
                {
                    _ = Consume();
                    ParseRVal();
                    Consume(']');
                    Consume('=');

                }
                else
                { //array style

                    CurrentExecutor.Constant(LuaObject.FromNumber(i++));
                }

                ParseRVal();

                next = Peek();
                if (next == ',')
                {

                    _ = Consume();
                    next = Peek();

                }
                else if (next != '}')
                {

                    Err("expecting ',' or '}'");
                }
            }

            CurrentExecutor.TableCreate(count);

            Consume('}');
        }

        protected LuaObject ParseNumberLiteral()
        {
            char next = Peek();
            StringBuilder sb = new();

            while (char.IsDigit(next) || next == '-' || next == '.' || next == 'e' || next == 'E')
            {
                _ = Consume();
                _ = sb.Append(next);
                next = Peek(true);
            }

            double val = double.Parse(sb.ToString(), CultureInfo.InvariantCulture);
            return LuaObject.FromNumber(val);
        }


        // FIXME: Handle multi line strings
        // http://www.lua.org/pil/2.4.html
        protected LuaObject ParseStringLiteral()
        {
            char next = Peek();
            StringBuilder sb = new();
            bool escaped = false;

            if (next is not '"' and not '\'')
            {
                Err("expected string");
            }

            char quote = next;

            _ = Consume();
            next = Consume();

            while (next != quote || escaped)
            {

                if (!escaped && next == '\\')
                {

                    escaped = true;

                }
                else if (escaped)
                {

                    switch (next)
                    {

                        case 'a': next = '\a'; break;
                        case 'b': next = '\b'; break;
                        case 'f': next = '\f'; break;
                        case 'n': next = '\n'; break;
                        case 'r': next = '\r'; break;
                        case 't': next = '\t'; break;
                        case 'v': next = '\v'; break;

                    }

                    _ = sb.Append(next);
                    escaped = false;
                }
                else
                {

                    _ = sb.Append(next);
                }

                next = Consume();
            }

            return LuaObject.FromString(sb.ToString());
        }

        protected string ParseIdentifier()
        {
            char next = Peek();
            StringBuilder sb = new();

            do
            {
                _ = Consume();
                _ = sb.Append(next);
                next = Peek(true);
            } while (char.IsLetterOrDigit(next) || next == '_');

            return sb.ToString();
        }

        protected void ParseComment()
        {
            Consume('-');
            if (Peek() == '-')
            {
                _ = Consume();
            }

            if (Consume() is '[' and '[')
            {
                while (!eof && (Consume() != ']' || Consume() != ']')) { /* consume entire block comment */; }
            }
            else
            {
                eof = input.ReadLine() == null;
                col = 1;
                row++;
            }
        }

        // scanner primitives:

        protected void Consume(char expected)
        {
            char actual = Peek();
            if (eof || actual != expected)
            {
                Err("expected '{0}'", expected);
            }

            while (Consume() != actual)
            { /* eat whitespace */ }

        }

        protected char Consume()
        {
            int r = input.Read();
            if (r == -1)
            {
                eof = true;
                return (char)0;
            }

            col++;
            return (char)r;
        }

        protected char Peek()
        {
            return Peek(false);
        }

        protected char Peek(bool whitespaceSignificant)
        {
        top:
            int p = input.Peek();
            if (p == -1)
            {
                eof = true;
                return (char)0;
            }

            char la = (char)p;

            if (!whitespaceSignificant)
            {
                if (la == '\r')
                {
                    _ = input.Read();
                    if ((char)input.Peek() == '\n')
                    {
                        _ = input.Read();
                    }

                    col = 1;
                    row++;
                    goto top;
                }

                if (la == '\n')
                {
                    _ = input.Read();
                    col = 1;
                    row++;
                    goto top;
                }

                if (char.IsWhiteSpace(la))
                {
                    _ = Consume();
                    goto top;
                }
            }

            return la;
        }


        // convenience methods:


        protected void Err(string message, params object[] args)
        {
            _ = Consume();
            throw new LuaException(file_name, row, col - 1, string.Format(message, args));
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool isManaged)
        {
            if (closeInputStream)
            {
                input.Close();
            }
        }

        #endregion
    }

    public class LuaException : Exception
    {

        public LuaException(string file, int row, int col, string message)
            : base(string.Format("Error in {0}({1},{2}): {3}", file, row, col, message))
        {
        }

        public LuaException(string message)
            : base("Error (unknown context): " + message)
        {
        }
    }
}

