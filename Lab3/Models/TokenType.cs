namespace Lab3.Models
{
    /// <summary>
    /// Типы лексем (например, while, do, end, until, loop, and, or, rel, as (=), ao (+/-), var, const, unknown).
    /// </summary>
    public enum TokenType
    {
        // Циклы
        While,
        Do,
        End,
        Until,
        Loop,

        // Логические
        And,
        Or,
        Rel,  // <, <=, ==, <>

        // Присваивание
        As,   // '='

        // Арифметика
        Ao,   // +, -

        // Идентификаторы, Константы
        Var,
        Const,

        Unknown
    }
}
