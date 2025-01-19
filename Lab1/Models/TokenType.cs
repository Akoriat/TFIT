namespace Lab1.Models
{
    /// <summary>
    /// Тип лексем (ключевые слова, операции и т. д.).
    /// </summary>
    public enum TokenType
    {
        // Ключевые слова
        While,
        Do,
        End,
        And,
        Or,
        Not,
        Until,
        Loop,
        Output,

        // Операции (сравнения, арифметические и т.д.)
        Rel,       // <, >, =, <>
        As,        // операция присваивания (если нужно)
        Ao,        // операции +, -
        AoMulDiv,  // операции *, /

        // Идентификатор, Константа
        Identifier,
        Constant,

        // Пропускные символы или неизвестное
        Whitespace,
        Unknown


    }
}
