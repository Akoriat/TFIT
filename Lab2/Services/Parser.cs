using Lab2.Models;

namespace Lab2.Services;

public class Parser
{
    private Token? _p;
    public List<string> ErrorMessages { get; } = new List<string>();

    public Parser(Token? startToken)
    {
        _p = startToken;
    }

    private void Error(string msg, int pos)
    {
        string error = $"Ошибка в позиции {pos}: {msg}";
        ErrorMessages.Add(error);
        Console.WriteLine(error);
    }

    public bool DoUntil(bool isTopLevel = false)
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

        if (isTopLevel)
        {
            if (_p != null && _p.Type != TokenType.EndOfFile)
            {
                Error("Лишние символы", _p.Position);
                return false;
            }
        }
        return true;
    }

    public bool Operators()
    {
        if (!Statement())
            return false;

        while (_p != null && _p.Type == TokenType.Del)
        {
            _p = _p.Next;
            if (_p != null &&
               (_p.Type == TokenType.Identifier || _p.Type == TokenType.Output || _p.Type == TokenType.Do))
            {
                if (!Statement())
                    return false;
            }
            else
            {
                break;
            }
        }
        return true;
    }

    public bool Statement()
    {
        if (_p == null)
        {
            Error("Ожидается оператор", 0);
            return false;
        }

        if (_p.Type == TokenType.Do)
        {
            return DoUntil();
        }
        else if (_p.Type == TokenType.Identifier)
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
            Error("Ожидается идентификатор, 'Output' или 'Do'", _p.Position);
            return false;
        }
    }

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