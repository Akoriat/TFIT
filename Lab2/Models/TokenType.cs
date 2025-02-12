namespace Lab2.Models
{
    // Перечисление типов токенов (лексем)
    public enum TokenType
    {
        Do,
        Until,
        Loop,
        Output,
        Not,
        And,
        Or,
        Identifier,
        Constant,
        Rel,
        As,
        Plus,
        Minus,
        Multiply,
        Divide,
        Delimiter,
        Unknown
    }
}
