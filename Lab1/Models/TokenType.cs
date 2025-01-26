namespace Lab1.Models
{
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

        // Операции
        Rel,       // <, >, =, <>
        As,        // операция присваивания
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
