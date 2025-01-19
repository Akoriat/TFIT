namespace Lab3.Models
{
    /// <summary>
    /// Описание лексемы: (Type, Lexeme, Position, Index).
    /// Index — при необходимости, если нужно хранить ссылку в таблице Var/Const.
    /// </summary>
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public int Position { get; set; }
        public int Index { get; set; }

        public Token(TokenType type, string lexeme, int position, int index = -1)
        {
            Type = type;
            Lexeme = lexeme;
            Position = position;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Type}('{Lexeme}') at pos={Position}";
        }
    }
}
