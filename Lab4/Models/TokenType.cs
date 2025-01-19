namespace Lab4.Models
{
    /// <summary>
    /// Типы лексем нашего языка
    /// </summary>
    public enum TokenType
    {
        // Циклы
        While,
        Do,
        End,
        // do until ... loop
        Until,
        Loop,

        // Логические
        And,
        Or,
        Rel,   // <, >, <=, ==, <>

        // Присваивание
        As,    // '='

        // Арифметические операции (+, -) (можно расширить на * /)
        Ao,

        // Идентификаторы / Константы
        Var,
        Const,

        Unknown
    }
}
