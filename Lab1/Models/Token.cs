namespace Lab1.Models
{
    // Класс для представления лексемы
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        /// <summary>
        /// Позиция в исходном тексте (индекс первого символа лексемы)
        /// </summary>
        public int Position { get; set; }
    }
}
