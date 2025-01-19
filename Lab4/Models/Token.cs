namespace Lab4.Models
{
    /// <summary>
    /// Описание лексемы (тип, текст, позиция, индекс для Var/Const).
    /// </summary>
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public int Position { get; set; }
        public int Index { get; set; }  // Для Var/Const - индекс в таблице

        public Token(TokenType type, string lexeme, int position, int index = -1)
        {
            Type = type;
            Lexeme = lexeme;
            Position = position;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Type}('{Lexeme}') at {Position}";
        }
    }
}
