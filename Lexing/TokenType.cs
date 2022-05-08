namespace Point.Lexing
{
    public enum TokenType
    {
        Identifier, Interger, String, Boolean,
        Variable,

        /// <summary>
        /// (
        /// </summary>
        Left_Parenthesis,
        /// <summary>
        /// )
        /// </summary>
        Right_Parenthesis,
        /// <summary>
        /// {
        /// </summary>
        Left_Brace,
        /// <summary>
        /// }
        /// </summary>
        Right_Brace,
        /// <summary>
        /// [
        /// </summary>
        Left_Square,
        /// <summary>
        /// ]
        /// </summary>
        Right_Square,
        /// <summary>
        /// &lt
        /// </summary>
        Left_Angle,
        /// <summary>
        /// &gt
        /// </summary>
        Right_Angle,

        Comma, Period, Plus, Minus, Star, Slash,
        Colon, Ampersand, Pipe, Equal, Not, Hash,
        Aspersand,

        Not_Equal, Equal_Equal, Greater_Equal, 
        Less_Equal, Plus_Equal, Minus_Equal, Star_Equal,
        Slash_Equal, Colon_Colon, Ampersand_Ampersand,
        Pipe_Pipe,

        Null, True, False, Import, As, Attach, Print,

        Semicolon, End
    }
}