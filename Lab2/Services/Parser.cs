using Lab2.Models;

namespace Lab2.Services;

// <DoUntil>      → Do Until <LogicalExpr> <Operators> Loop
// <Operators>    → <Statement> { Del <Statement> }
// <Statement>    → Identifier As <ArithExpr>
//                | Output <Operand>
// <ArithExpr>    → <Term> { Ao <Term> }
// <Term>         → <Factor> { (Mul | Div) <Factor> }
// <Factor>       → Identifier | Constant
// <LogicalExpr>  → <LogTerm> { Or <LogTerm> }
// <LogTerm>      → <FactorLog> { And <FactorLog> }
// <FactorLog>    → Not <FactorLog>
//                | <RelExpr>
// <RelExpr>      → <Operand> [ Rel <Operand> ]
// <Operand>      → Identifier | Constant

public class Parser
{
    private Token? _p;
    public List<string> ErrorMessages { get; } = [];

    public Parser(Token? startToken)
    {
        _p = startToken;
    }

    private void Error(string msg, int pos)
    {
        var error = $"Ошибка в позиции {pos}: {msg}";
        ErrorMessages.Add(error);
        Console.WriteLine(error);
    }

    // <DoUntil> → Do Until <LogicalExpr> <Operators> Loop
    public bool DoUntil()
    {
        if (_p == null || _p.Type != TokenType.Do)
        {
            Error("Ожидается 'Do'", _p?.Position ?? 0);
            return false;
        }
        _p = _p.Next;

        if (_p == null || _p.Type != TokenType.Until)
        {
            Error("Ожидается 'Until'", _p?.Position ?? 0);
            return false;
        }
        _p = _p.Next;

        if (!LogicalExpr())
            return false;

        if (!Operators())
            return false;

        if (_p == null || _p.Type != TokenType.Loop)
        {
            Error("Ожидается 'Loop'", _p?.Position ?? 0);
            return false;
        }
        _p = _p.Next;

        if (_p != null && _p.Type != TokenType.EndOfFile)
        {
            Error("Лишние символы", _p.Position);
            return false;
        }
        return true;
    }

    // <Operators> → <Statement> { Del <Statement> } [ Del ]
    public bool Operators()
    {
        // Обязательно должно быть как минимум одно Statement
        if (!Statement())
            return false;

        // Обработка последовательности "Del <Statement>"
        while (_p != null && _p.Type == TokenType.Del)
        {
            // Сначала потребляем разделитель Del
            _p = _p.Next;

            // Если следующий токен допускает начало оператора (Identifier или Output),
            // то разбираем очередной Statement
            if (_p != null && (_p.Type == TokenType.Identifier || _p.Type == TokenType.Output))
            {
                if (!Statement())
                    return false;
            }
            else
            {
                // Иначе опциональный завершающий Del принят, выходим из цикла
                break;
            }
        }
        return true;
    }

    // <Statement> → Identifier As <ArithExpr> | Output <Operand>
    public bool Statement()
    {
        if (_p == null)
        {
            Error("Ожидается оператор", 0);
            return false;
        }

        if (_p.Type == TokenType.Identifier)
        {
            _p = _p.Next;
            if (_p == null || _p.Type != TokenType.As)
            {
                Error("Ожидается 'As'", _p?.Position ?? 0);
                return false;
            }
            _p = _p.Next;
            if (!ArithExpr())
                return false;
            return true;
        }
        else if (_p.Type == TokenType.Output)
        {
            _p = _p.Next;
            if (!Operand())
                return false;
            return true;
        }
        else
        {
            Error("Ожидается идентификатор или 'Output'", _p.Position);
            return false;
        }
    }

    // <ArithExpr> → <Term> { Ao <Term> }
    public bool ArithExpr()
    {
        if (!Term())
            return false;
        while (_p != null && _p.Type == TokenType.Ao)
        {
            _p = _p.Next;
            if (!Term())
                return false;
        }
        return true;
    }

    // <Term> → <Factor> { (Mul | Div) <Factor> }
    public bool Term()
    {
        if (!Factor())
            return false;
        while (_p != null && (_p.Type == TokenType.Mul || _p.Type == TokenType.Div))
        {
            _p = _p.Next;
            if (!Factor())
                return false;
        }
        return true;
    }

    // <Factor> → Identifier | Constant
    public bool Factor()
    {
        if (_p == null || (_p.Type != TokenType.Identifier && _p.Type != TokenType.Constant))
        {
            Error("Ожидается идентификатор или константа", _p?.Position ?? 0);
            return false;
        }
        _p = _p.Next;
        return true;
    }

    // <LogicalExpr> → <LogTerm> { Or <LogTerm> }
    public bool LogicalExpr()
    {
        if (!LogTerm())
            return false;
        while (_p != null && _p.Type == TokenType.Or)
        {
            _p = _p.Next;
            if (!LogTerm())
                return false;
        }
        return true;
    }

    // <LogTerm> → <FactorLog> { And <FactorLog> }
    public bool LogTerm()
    {
        if (!FactorLog())
            return false;
        while (_p != null && _p.Type == TokenType.And)
        {
            _p = _p.Next;
            if (!FactorLog())
                return false;
        }
        return true;
    }

    // <FactorLog> → Not <FactorLog> | <RelExpr>
    public bool FactorLog()
    {
        if (_p != null && _p.Type == TokenType.Not)
        {
            _p = _p.Next;
            return FactorLog();
        }
        else
        {
            return RelExpr();
        }
    }

    // <RelExpr> → <Operand> [ Rel <Operand> ]
    public bool RelExpr()
    {
        if (!Operand())
            return false;
        if (_p != null && _p.Type == TokenType.Rel)
        {
            _p = _p.Next;
            if (!Operand())
                return false;
        }
        return true;
    }

    // <Operand> → Identifier | Constant
    public bool Operand()
    {
        if (_p == null || (_p.Type != TokenType.Identifier && _p.Type != TokenType.Constant))
        {
            Error("Ожидается идентификатор или константа", _p?.Position ?? 0);
            return false;
        }
        _p = _p.Next;
        return true;
    }
}