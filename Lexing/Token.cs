namespace Point.Lexing
{
    [Serializable] public class Token
    {
        public readonly TokenType Type;
        public readonly string Raw;
        public readonly object Literal;
        public readonly int Line;

        public Token(TokenType type, string raw, object value, int line)
        {
            Type = type;
            Raw = raw;
            Literal = value;
            Line = line;
        }

        public Token(string raw)
        {
            Type = TokenType.Identifier;
            Raw = raw;
            Literal = null;
            Line = -1;
        }

        public override string ToString()
        {
            return $"{Type} {Raw} {Literal}";
        }

        public static implicit operator Token(string raw)
            => new(raw);
    }
}