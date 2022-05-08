//using Point.Interpreting;
using Point.Lexing;

namespace Point.Parsing
{
    public class Parser
    {
        private class ParseError : Exception { }
        public bool AtEnd => Peek().Type == TokenType.End;
        private readonly List<Token> Tokens;
        private int Current = 0;

        public Parser(List<Token> tokens)
        {
            Tokens = new();
            foreach (Token? item in tokens) // Remove empty lines
            {
                Tokens.Add(item);
            }
        }
        public List<Stmt> Parse()
        {
            List<Stmt> statements = new();
            while (!AtEnd)
            {
                statements.Add(Declaration());
            }

            return statements;
        }
        private Expr Expression()
        {
            return ExprAssignment();
        }
        private Expr ExprAssignment()
        {
            Expr expr = ExprOr();

            if (Match(TokenType.Equal))
            {
                Token eq = Previous();
                Expr value = ExprAssignment();

                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }

                Error(eq, "Invalid assignment target");
            }

            return expr;
        }
        private Expr ExprOr()
        {
            Expr expr = ExprAnd();

            while (Match(TokenType.Pipe_Pipe))
            {
                Token op = Previous();
                Expr right = ExprAnd();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }
        private Expr ExprAnd()
        {
            Expr expr = ExprEquality();

            while (Match(TokenType.Ampersand_Ampersand))
            {
                Token op = Previous();
                Expr right = ExprEquality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }
        private Expr ExprEquality()
        {
            Expr expr = ExprTerm();

            while (Match(TokenType.Not_Equal, TokenType.Equal_Equal))
            {
                Token op = Previous();
                Expr right = ExprTerm();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }
        private Expr ExprTerm()
        {
            Expr expr = ExprFactor();

            while (Match(TokenType.Plus, TokenType.Minus))
            {
                Token op = Previous();
                Expr right = ExprFactor();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }
        private Expr ExprFactor()
        {
            Expr expr = ExprUnary();

            while (Match(TokenType.Slash, TokenType.Star))
            {
                Token op = Previous();
                Expr right = ExprUnary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }
        private Expr ExprUnary()
        {
            if (Match(TokenType.Not, TokenType.Minus))
            {
                Token op = Previous();
                Expr right = ExprUnary();
                return new Expr.Unary(op, right);
            }

            return ExprCall();
        }
        private Expr ExprCall()
        {
            Expr expr = ExprPrimary();

            while (true)
            {
                if (Match(TokenType.Left_Parenthesis))
                {
                    expr = ExprFinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }
        private Expr ExprFinishCall(Expr callee)
        {
            List<Expr> arguments = new();
            if (!Check(TokenType.Right_Parenthesis))
            {
                do
                {
                    if (arguments.Count >= 255)
                        Error(Peek(), "Maximum of 255 arguments allowed");

                    arguments.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            Token paren = Consume(TokenType.Right_Parenthesis, "Expected ')' after arguments");
            ExpectEnd();

            return null;
        }
        private Expr ExprPrimary()
        {
            if (Match(TokenType.True)) return new Expr.Literal(true);
            if (Match(TokenType.False)) return new Expr.Literal(false);
            if (Match(TokenType.Null)) return new Expr.Literal(null);
            if (Match(TokenType.String, TokenType.Interger))
                return new Expr.Literal(Previous().Literal);
            if (Match(TokenType.Left_Parenthesis))
            {
                Expr expr = Expression();
                Consume(TokenType.Right_Parenthesis, "Expected ')' after expression");
                return new Expr.Grouping(expr);
            }
            if (Match(TokenType.Variable))
            {
                return new Expr.Variable(Previous());
            }

            //if (Check(TokenType.Identifier)) return CreateElement(true);
            //if (Check(TokenType.Period)) return CreateElement(true);
            //if (Check(TokenType.Hash)) return CreateElement(true);

            throw Error(Peek(), "Expected expression");
        }
        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }
        private bool Check(TokenType type)
        {
            if (AtEnd) return false;
            return Peek().Type == type;
        }
        private Token Consume(TokenType type, string message, bool allowPrematureEnd = false)
        {
            if (Check(type) || (allowPrematureEnd && AtEnd)) return Advance();

            throw Error(Peek(), message);
        }
        private Token Advance()
        {
            if (!AtEnd) Current++;
            return Previous();
        }
        private Token Peek(int n = 0)
            => Tokens[Current + n];
        private Token Previous()
            => Tokens[Current - 1];
        private ParseError Error(Token token, string message)
        {
            Global.Error(token, message);
            return new ParseError();
        }
        private void Synchronize()
        {
            Advance();

            while (!AtEnd)
            {
                if (Previous().Type == TokenType.Semicolon) return;

                //switch (Peek().Type)
                //{
                //    case TokenType.If:
                //    case TokenType.Else:
                //    case TokenType.Elif:
                //    case TokenType.For:
                //        return;
                //}

                Advance();
            }
        }
        private Stmt Declaration()
        {
            try
            {
                //if (Match(TokenType.Function)) return FunctionDeclaration(VariableType.Normal);

                return Statement();
            }
            catch (ParseError error)
            {
                Synchronize();
                return null;
            }
        }
        private Stmt Statement()
        {
            if (Match(TokenType.Print)) return PrintStatement();
            if (Match(TokenType.Import)) return ImportStatement();
            if (Match(TokenType.Attach)) return AttachStatement();
            if (Match(TokenType.Left_Brace)) return new Stmt.Block(Block());

            if (Check(TokenType.Identifier)) return new Stmt.Expression(CreateElement(true));
            if (Check(TokenType.Period)) return new Stmt.Expression(CreateElement(true));
            if (Check(TokenType.Hash)) return new Stmt.Expression(CreateElement(true));

            return ExpressionStatement();
        }
        private List<Stmt> Block()
        {
            List<Stmt> statements = new();
            ExpectEnd();
            while (!Check(TokenType.Right_Brace))
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.Right_Brace, "Expected '}' after block");
            ExpectEnd();
            return statements;
        }
        private Expr CreateElement(bool toplevel)
        {
            GetElementHeaders(out string id, out string tag, out List<string> classes);
            GetElementBody(out Dictionary<string, Expr> attributes, out List<Expr> content);

            return new Expr.Element(tag, id, classes, attributes, content, toplevel);
        }
        private void GetElementHeaders(out string id, out string tag, out List<string> classes)
        {
            string _id = "";
            string _tag = "div";
            List<string> _classes = new();

            while (!Check(TokenType.Left_Brace))
            {
                Token tok = Advance();
                if (tok.Type == TokenType.Identifier)
                {
                    _tag = tok.Raw;
                }
                else if (tok.Type == TokenType.Period)
                {
                    _classes.Add(Advance().Raw);
                }
                else if (tok.Type == TokenType.Hash)
                {
                    _id = Advance().Raw;
                }
            }

            id = _id;
            tag = _tag;
            classes = _classes;
        }
        private void GetElementBody(out Dictionary<string, Expr> attributes, out List<Expr> content)
        {
            Dictionary<string, Expr> _attributes = new();
            List<Expr> _content = new();

            Consume(TokenType.Left_Brace, "Expected '{'");

            while (!Check(TokenType.Right_Brace))
            {
                Token token = Peek();
                Token secToken = Peek(1);
                if (secToken.Type == TokenType.Semicolon)
                {
                    _content.Add(Expression());
                    ExpectEnd();
                }
                else if (secToken.Type == TokenType.Colon)
                {
                    Advance();
                    Advance();
                    _attributes[token.Raw] = Expression();
                    ExpectEnd();
                }
                else if (secToken.Type == TokenType.Left_Brace
                    || secToken.Type == TokenType.Identifier
                    || secToken.Type == TokenType.Period
                    || secToken.Type == TokenType.Hash)
                {
                    _content.Add(CreateElement(false));
                }
                else
                {
                    _content.Add(Expression());
                    ExpectEnd();
                }
            }

            Consume(TokenType.Right_Brace, "Expected '}'");

            attributes = _attributes;
            content = _content;
        }
        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            ExpectEnd();
            return new Stmt.Expression(expr);
        }
        private Stmt ImportStatement()
        {
            Token name = Consume(TokenType.String, "Expected string");
            Token type = null;
            if (Peek().Type == TokenType.As)
            {
                Consume(TokenType.As, "Expected 'as'");
                type = Consume(TokenType.String, "Expected string");
            }
            ExpectEnd();
            return new Stmt.Import(name, type);
        }
        private Stmt AttachStatement()
        {
            Token name = Consume(TokenType.String, "Expected string");
            ExpectEnd();
            return new Stmt.Attach(name);
        }
        private Stmt PrintStatement()
        {
            Expr expression = Expression();
            ExpectEnd();
            return new Stmt.Print(expression);
        }
        private void ExpectEnd()
        {
            Consume(TokenType.Semicolon, "Expected line to end", false);
        }
    }
}
