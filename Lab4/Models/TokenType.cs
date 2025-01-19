namespace Lab4.Models
{
    public enum TokenType
    {
        // Операторы цикла
        While,
        Do,
        End,
        Until,
        Loop,

        // Логические
        And,
        Or,
        Rel,     // <, <=, ==, <>

        // Присваивание
        As,      // '='

        // Арифметические операции (+, -)
        Ao,

        // Идентификаторы, Константы
        Var,
        Const,

        Unknown
    }
}
