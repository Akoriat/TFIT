namespace Lab1.Models
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }  // Текст лексемы
        public int Position { get; set; }   // Начальная позиция
        public int Index { get; set; }      // Индекс в таблице

        public Token(TokenType type, string lexeme, int position, int index = -1)
        {
            Type = type;
            Lexeme = lexeme;
            Position = position;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Type} (\"{Lexeme}\") pos={Position}, index={Index}";
        }
    }
}
