using System.Diagnostics;

namespace Point.Lexing
{
    public class Lexer
    {
        public string Source;
        public bool AtEnd => current >= Source.Length;
        private readonly List<Token> tokens;
        private int start;
        private int current;
        private int line;
        private static readonly Dictionary<string, TokenType> keywords = new()
        {
            { "null", TokenType.Null },
            { "true", TokenType.True },
            { "false", TokenType.False },
            { "import", TokenType.Import },
            { "as", TokenType.As },
            { "attach", TokenType.Attach },
            { "print", TokenType.Print },
        };

        public Lexer(string source)
        {
            Source = source;
            tokens = new();
            start = 0;
            current = 0;
            line = 1;
        }

        public List<Token> Lex()
        {
            while (!AtEnd)
            {
                start = current;
                LexToken();
            }

            tokens.Add(new(TokenType.End, "", null, line));
            return tokens;
        }

        private void LexToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.Left_Parenthesis); break;
                case ')': AddToken(TokenType.Right_Parenthesis); break;
                case '{': AddToken(TokenType.Left_Brace); break;
                case '}': AddToken(TokenType.Right_Brace); break;
                case '[': AddToken(TokenType.Left_Square); break;
                case ']': AddToken(TokenType.Right_Square); break;
                case '<': AddToken(TokenType.Left_Angle); break;
                case '>': AddToken(TokenType.Right_Angle); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Period); break;
                case '#': AddToken(TokenType.Hash); break;
                case '@': AddToken(TokenType.Aspersand); break;

                case '&': AddToken(Match('&') ? TokenType.Ampersand_Ampersand : TokenType.Ampersand); break;
                case '|': AddToken(Match('|') ? TokenType.Pipe_Pipe : TokenType.Pipe); break;
                case '=': AddToken(Match('=') ? TokenType.Equal_Equal : TokenType.Equal); break;
                case '!': AddToken(Match('=') ? TokenType.Not_Equal : TokenType.Not); break;
                case '+': AddToken(Match('=') ? TokenType.Plus_Equal : TokenType.Plus); break;
                case '-': AddToken(Match('=') ? TokenType.Minus_Equal : TokenType.Minus); break;
                case '*': AddToken(Match('=') ? TokenType.Star_Equal : TokenType.Star); break;
                case '/': //AddToken(Match('=') ? TokenType.Slash_Equal : TokenType.Slash); break;
                    if (Match('/'))
                        while (Peek() != '\n' && !AtEnd) Advance();
                    if (Match('*'))
                        while (Peek() != '*' && Peek(1) != '/' && !AtEnd) Advance();
                    else if (Match('='))
                        AddToken(TokenType.Slash_Equal);
                    else
                        AddToken(TokenType.Slash);
                    break;

                case ';':
                    AddToken(TokenType.Semicolon);
                    break;

                case ':':
                    AddToken(TokenType.Colon);
                    break;

                // Trim useless characters
                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n':
                    line++;
                    break;

                // Literals
                case '\'':
                case '\"':
                    HandleString(c);
                    break;

                case '$':
                    HandleVariable();
                    break;

                default:
                    if (char.IsDigit(c))
                        HandleInterger();
                    else if (char.IsLetterOrDigit(c) || c == '_')
                        HandleIdentifier();
                    else
                    {
                        Global.Error(line, $"Unexpected character '{c}'");
                    }
                    break;
            }
        }

        private char Advance()
        {
            current++;
            return Source[current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object value)
        {
            string text = Source.Between(start, current);
            tokens.Add(new(type, text, value, line));
        }

        private bool Match(char c)
        {
            if (AtEnd) return false;
            if (Source[current] != c) return false;

            current++;
            return true;
        }
        private char Peek(int n = 0)
        {
            if (current + n >= Source.Length) return '\0';
            return Source[current + n];
        }
        private void HandleString(char st)
        {
            string text = "";
            bool escaped = false;
            while (!AtEnd)
            {
                char ch = Peek();
                if (ch == '\n') line++;
                if (ch == st && !escaped) break;
                if (escaped)
                {
                    switch (ch)
                    {
                        case 'a': ch = '\a'; break;
                        case 'b': ch = '\b'; break;
                        case 'f': ch = '\f'; break;
                        case 'n': ch = '\n'; break;
                        case 'r': ch = '\r'; break;
                        case 't': ch = '\t'; break;
                        case 'v': ch = '\v'; break;
                        case '0': ch = '\0'; break;

                        case '\\':
                        case '\'':
                        case '\"': 
                            break;

                        default:
                            Global.Error(line, "Unknown escape character"); 
                            break;
                    }
                }
                if (escaped) escaped = false;
                Advance();
                if (ch == '\\' && !escaped)
                {
                    escaped = true;
                    continue;
                }

                text += ch;
            }

            if (AtEnd)
            {
                Global.Error(line, "Unterminated string");
                return;
            }

            Advance();

            //Source.Between(start + 1, current - 1);
            AddToken(TokenType.String, text);
        }
        private void HandleInterger()
        {
            while (char.IsDigit(Peek())) Advance();

            if (Peek() == '.' && char.IsDigit(Peek(1)))
            {
                Advance();

                while (char.IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.Interger, double.Parse(Source.Between(start, current)));
        }
        private void HandleIdentifier()
        {
            while (ValidIdentifierChar(Peek())) Advance();

            string text = Source.Between(start, current);
            TokenType type = keywords.ContainsKey(text) ? keywords[text] : TokenType.Identifier;
            AddToken(type);
        }
        private void HandleVariable()
        {
            while (ValidIdentifierChar(Peek())) Advance();

            string text = Source.Between(start, current);
            AddToken(TokenType.Variable, text);
        }
        private bool ValidIdentifierChar(char c)
            => char.IsLetterOrDigit(c) || c == '_' || c == '-';
    }

    public static class Extensions
    {
        public static string Between(this string s, int startIndex, int endIndex)
        {
            int length = endIndex - startIndex;
            return s.Substring(startIndex, length);
        }
    }
}