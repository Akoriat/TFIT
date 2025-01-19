namespace Lab3.Models
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public int Position { get; set; }
        public int Index { get; set; }  // для Var/Const (ссылка в таблицу)
        public Token(TokenType type, string lexeme, int pos, int index = -1)
        {
            Type = type;
            Lexeme = lexeme;
            Position = pos;
            Index = index;
        }
        public override string ToString()
        {
            return $"{Type}('{Lexeme}') at {Position}";
        }
    }
}
