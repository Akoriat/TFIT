namespace Lab2.Models
{
    public class Token
    {
        public TokenType Type;
        public string Lexeme = "";
        public int Position;
        public LexemeCategory Category;
        public Token? Next = null;
    }
}