namespace Lab2.Models
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }

        public Token() { }
        public Token(TokenType type, string lexeme, int startPos, int endPos)
        {
            Type = type;
            Lexeme = lexeme;
            StartPos = startPos;
            EndPos = endPos;
        }

        public override string ToString() => $"{Lexeme} ({Type})";
    }
}
