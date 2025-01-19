namespace Lab1.Models
{
    /// <summary>
    /// Класс, описывающий лексему.
    /// </summary>
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }  // Текст лексемы
        public int Position { get; set; }   // Начальная позиция в исходном тексте
        public int Index { get; set; }      // Индекс в таблице (для идентификаторов/констант)

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
