namespace Lab2.Models
{
    // Класс Token описывает отдельную лексему с типом, текстом и позицией во входном потоке
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public int Position { get; set; }

        public Token(TokenType type, string lexeme, int position)
        {
            Type = type;
            Lexeme = lexeme;
            Position = position;
        }

        public override string ToString() => $"{Lexeme} ({Type})";
    }
}
