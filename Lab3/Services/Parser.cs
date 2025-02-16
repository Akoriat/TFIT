using Lab3.Models;

namespace Lab3.Services
{
    public class Parser
    {
        private Token? _p;
        public List<string> ErrorMessages { get; } = new List<string>();

        private List<PostfixEntry> _postfix = new();

        private List<string> VarTable = new List<string>();
        private List<string> ConstTable = new List<string>();
        public IReadOnlyList<string> Variables => VarTable;
        public IReadOnlyList<string> Constants => ConstTable;

        public List<PostfixEntry> Postfix => _postfix;

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

        private int WriteCmd(ECmd cmd)
        {
            PostfixEntry entry = new PostfixEntry { Type = EEntryType.EtCmd, Index = (int)cmd };
            _postfix.Add(entry);
            return _postfix.Count - 1;
        }

        private int WriteVar(string var)
        {
            int index = VarTable.IndexOf(var);
            if (index == -1)
            {
                VarTable.Add(var);
                index = VarTable.Count - 1;
            }
            PostfixEntry entry = new PostfixEntry { Type = EEntryType.EtVar, Index = index };
            _postfix.Add(entry);
            return _postfix.Count - 1;
        }

        private int WriteConst(string cons)
        {
            int index = ConstTable.IndexOf(cons);
            if (index == -1)
            {
                ConstTable.Add(cons);
                index = ConstTable.Count - 1;
            }
            PostfixEntry entry = new PostfixEntry { Type = EEntryType.EtConst, Index = index };
            _postfix.Add(entry);
            return _postfix.Count - 1;
        }

        private int WriteCmdPtr(int ptr)
        {
            PostfixEntry entry = new PostfixEntry { Type = EEntryType.EtCmdPtr, Index = ptr };
            _postfix.Add(entry);
            return _postfix.Count - 1;
        }

        private void SetCmdPtr(int ind, int ptr)
        {
            if (ind >= 0 && ind < _postfix.Count)
            {
                _postfix[ind].Index = ptr;
            }
        }

        public bool DoUntil(bool isTopLevel = false)
        {
            int indFirst = _postfix.Count;

            if (_p == null || _p.Type != TokenType.Do)
            {
                Error("Ожидается 'do'", _p?.Position ?? 0);
                return false;
            }
            _p = _p.Next;

            if (_p == null || _p.Type != TokenType.Until)
            {
                Error("Ожидается 'until'", _p?.Position ?? 0);
                return false;
            }
            _p = _p.Next;

            if (!LogicalExpr())
                return false;

            int jzPtrIndex = WriteCmdPtr(-1);
            WriteCmd(ECmd.JZ);

            if (!Operators())
                return false;

            if (_p == null || _p.Type != TokenType.Loop)
            {
                Error("Ожидается 'loop'", _p?.Position ?? 0);
                return false;
            }
            _p = _p.Next;

            WriteCmdPtr(indFirst);
            int indJMP = WriteCmd(ECmd.JMP);

            SetCmdPtr(jzPtrIndex, indJMP + 1);

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
                string varName = _p.Lexeme;
                WriteVar(varName);
                _p = _p.Next;
                if (_p == null || _p.Type != TokenType.As)
                {
                    Error("Ожидается '=' (as)", _p?.Position ?? 0);
                    return false;
                }
                _p = _p.Next;
                if (!ArithExpr())
                    return false;
                WriteCmd(ECmd.SET);
                return true;
            }
            else if (_p.Type == TokenType.Output)
            {
                _p = _p.Next;
                if (!Operand())
                    return false;
                WriteCmd(ECmd.OUT);
                return true;
            }
            else
            {
                Error("Ожидается идентификатор, 'output' или 'do'", _p.Position);
                return false;
            }
        }

        public bool ArithExpr()
        {
            if (!Term())
                return false;
            while (_p != null && _p.Type == TokenType.Ao)
            {
                ECmd cmd;
                if (_p.Lexeme == "+")
                    cmd = ECmd.ADD;
                else if (_p.Lexeme == "-")
                    cmd = ECmd.SUB;
                else
                {
                    Error("Неизвестная арифметическая операция", _p.Position);
                    return false;
                }
                _p = _p.Next;
                if (!Term())
                    return false;
                WriteCmd(cmd);
            }
            return true;
        }

        public bool Term()
        {
            if (!Factor())
                return false;
            while (_p != null && (_p.Type == TokenType.Mul || _p.Type == TokenType.Div))
            {
                ECmd cmd = _p.Type == TokenType.Mul ? ECmd.MUL : ECmd.DIV;
                _p = _p.Next;
                if (!Factor())
                    return false;
                WriteCmd(cmd);
            }
            return true;
        }

        public bool Factor()
        {
            if (_p == null)
            {
                Error("Ожидается идентификатор или константа", 0);
                return false;
            }
            if (_p.Type == TokenType.Identifier)
            {
                WriteVar(_p.Lexeme);
            }
            else if (_p.Type == TokenType.Constant)
            {
                WriteConst(_p.Lexeme);
            }
            else
            {
                Error("Ожидается идентификатор или константа", _p.Position);
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
                WriteCmd(ECmd.OR);
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
                WriteCmd(ECmd.AND);
            }
            return true;
        }

        public bool FactorLog()
        {
            if (_p != null && _p.Type == TokenType.Not)
            {
                _p = _p.Next;
                if (!FactorLog())
                    return false;
                WriteCmd(ECmd.NOT);
                return true;
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
                ECmd cmd;
                var op = _p.Lexeme;
                switch (op)
                {
                    case "<":
                        cmd = ECmd.CMPL;
                        break;
                    case "<>":
                        cmd = ECmd.CMPNE;
                        break;
                    case "==":
                        cmd = ECmd.CMPE;
                        break;
                    case ">":
                        cmd = ECmd.CMPG;
                        break;
                    default:
                        Error("Неизвестный оператор сравнения", _p.Position);
                        return false;
                }
                _p = _p.Next;
                if (!Operand())
                    return false;
                WriteCmd(cmd);
            }
            return true;
        }

        public bool Operand()
        {
            if (_p == null)
            {
                Error("Ожидается идентификатор или константа", 0);
                return false;
            }
            if (_p.Type == TokenType.Identifier)
            {
                WriteVar(_p.Lexeme);
            }
            else if (_p.Type == TokenType.Constant)
            {
                WriteConst(_p.Lexeme);
            }
            else
            {
                Error("Ожидается идентификатор или константа", _p.Position);
                return false;
            }
            _p = _p.Next;
            return true;
        }
    }
}
