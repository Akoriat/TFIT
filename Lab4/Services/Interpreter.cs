using System;
using System.Collections.Generic;
using System.Globalization;
using Lab4.Models;

namespace Lab4.Services
{
    public class Interpreter
    {
        private readonly List<PostfixEntry> _postfix;
        private readonly int[] _varValues;      // значения переменных
        private readonly Stack<int> _stack;
        private readonly ConstantTable _constTable;

        public Interpreter(List<PostfixEntry> postfix, int varCount, ConstantTable cTable)
        {
            _postfix = postfix;
            _stack = new Stack<int>();
            _varValues = new int[varCount];
            _constTable = cTable;
        }

        public void Run()
        {
            int pos = 0;
            while (pos < _postfix.Count)
            {
                PostfixEntry e = _postfix[pos];
                switch (e.type)
                {
                    case EEntryType.etCmd:
                        {
                            ECmd cmd = (ECmd)e.index;
                            switch (cmd)
                            {
                                case ECmd.JMP:
                                    {
                                        if (_stack.Count < 1)
                                            throw new Exception("Stack underflow on JMP");
                                        int adr = _stack.Pop();
                                        pos = adr;
                                        continue;
                                    }
                                case ECmd.JZ:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on JZ");
                                        int adr = _stack.Pop();
                                        int val = _stack.Pop();
                                        if (val == 0)
                                        {
                                            pos = adr;
                                            continue;
                                        }
                                    }
                                    break;
                                case ECmd.SET:
                                    {
                                        // var, val
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on SET");
                                        int val = _stack.Pop();
                                        int varIndex = _stack.Pop();
                                        _varValues[varIndex] = val;
                                    }
                                    break;
                                case ECmd.ADD:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on ADD");
                                        int v2 = _stack.Pop();
                                        int v1 = _stack.Pop();
                                        _stack.Push(v1 + v2);
                                    }
                                    break;
                                case ECmd.SUB:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on SUB");
                                        int v2 = _stack.Pop();
                                        int v1 = _stack.Pop();
                                        _stack.Push(v1 - v2);
                                    }
                                    break;
                                case ECmd.MUL:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on MUL");
                                        int v2 = _stack.Pop();
                                        int v1 = _stack.Pop();
                                        _stack.Push(v1 * v2);
                                    }
                                    break;
                                case ECmd.DIV:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on DIV");
                                        int v2 = _stack.Pop();
                                        int v1 = _stack.Pop();
                                        if (v2 == 0)
                                            throw new DivideByZeroException();
                                        _stack.Push(v1 / v2);
                                    }
                                    break;
                                case ECmd.AND:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on AND");
                                        int v2 = _stack.Pop();
                                        int v1 = _stack.Pop();
                                        // 0 = false, !=0 = true
                                        _stack.Push((v1 != 0 && v2 != 0) ? 1 : 0);
                                    }
                                    break;
                                case ECmd.OR:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on OR");
                                        int v2 = _stack.Pop();
                                        int v1 = _stack.Pop();
                                        _stack.Push((v1 != 0 || v2 != 0) ? 1 : 0);
                                    }
                                    break;
                                case ECmd.NOT:
                                    {
                                        if (_stack.Count < 1)
                                            throw new Exception("Stack underflow on NOT");
                                        int v = _stack.Pop();
                                        _stack.Push(v == 0 ? 1 : 0);
                                    }
                                    break;
                                case ECmd.CMPE:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on CMPE");
                                        int b = _stack.Pop();
                                        int a = _stack.Pop();
                                        _stack.Push(a == b ? 1 : 0);
                                    }
                                    break;
                                case ECmd.CMPNE:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on CMPNE");
                                        int b = _stack.Pop();
                                        int a = _stack.Pop();
                                        _stack.Push(a != b ? 1 : 0);
                                    }
                                    break;
                                case ECmd.CMPL:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on CMPL");
                                        int b = _stack.Pop();
                                        int a = _stack.Pop();
                                        _stack.Push(a < b ? 1 : 0);
                                    }
                                    break;
                                case ECmd.CMPLE:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on CMPLE");
                                        int b = _stack.Pop();
                                        int a = _stack.Pop();
                                        _stack.Push(a <= b ? 1 : 0);
                                    }
                                    break;
                                default:
                                    throw new Exception($"Неизвестная команда: {cmd}");
                            }
                            pos++;
                        }
                        break;
                    case EEntryType.etVar:
                        // Кладём индекс переменной в стек
                        _stack.Push(e.index);
                        pos++;
                        break;
                    case EEntryType.etConst:
                        {
                            // Превращаем текст константы в int
                            string sVal = _constTable.GetConstant(e.index);
                            int val = int.Parse(sVal, CultureInfo.InvariantCulture);
                            _stack.Push(val);
                            pos++;
                        }
                        break;
                    case EEntryType.etCmdPtr:
                        // Просто кладём адрес (число) в стек
                        _stack.Push(e.index);
                        pos++;
                        break;
                }
            }
        }

        public int GetVarValue(int varIndex)
        {
            return _varValues[varIndex];
        }

        public void SetVarValue(int varIndex, int val)
        {
            _varValues[varIndex] = val;
        }
    }
}
