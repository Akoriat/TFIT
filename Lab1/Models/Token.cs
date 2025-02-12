namespace Lab1.Models
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
    }
}
