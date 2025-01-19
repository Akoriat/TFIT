using System;
using System.Collections.Generic;
using System.Globalization;
using Lab4.Models;

namespace Lab4.Services
{
    public class Interpreter
    {
        private readonly List<PostfixEntry> _postfix;
        private readonly int[] _varValues; // хранит значения всех переменных
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
                                // Новая команда LOAD
                                case ECmd.LOAD:
                                    {
                                        // LOAD и следующая запись etCmdPtr(varIndex)
                                        if (pos + 1 >= _postfix.Count)
                                            throw new Exception("No CmdPtr after LOAD");
                                        PostfixEntry nextE = _postfix[pos + 1];
                                        if (nextE.type != EEntryType.etCmdPtr)
                                            throw new Exception("LOAD expects next entry = etCmdPtr(varIndex)");

                                        int vIndex = nextE.index;
                                        if (vIndex < 0 || vIndex >= _varValues.Length)
                                            throw new Exception($"varIndex={vIndex} вне массива (0..{_varValues.Length - 1})");

                                        int currentVal = _varValues[vIndex];
                                        _stack.Push(currentVal);

                                        pos += 2;
                                        continue;
                                    }

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
                                        // stack: [varIndex, val]
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on SET");
                                        int value = _stack.Pop();
                                        int varIndex = _stack.Pop();

                                        if (varIndex < 0 || varIndex >= _varValues.Length)
                                            throw new Exception($"varIndex={varIndex} вне (0..{_varValues.Length - 1})");

                                        _varValues[varIndex] = value;
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
                                case ECmd.AND:
                                    {
                                        if (_stack.Count < 2)
                                            throw new Exception("Stack underflow on AND");
                                        int v2 = _stack.Pop();
                                        int v1 = _stack.Pop();
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
                        {
                            // Это "адрес переменной" (индекс) - нужно для SET 
                            // => на стеке окажется varIndex.
                            _stack.Push(e.index);
                            pos++;
                        }
                        break;

                    case EEntryType.etConst:
                        {
                            string sVal = _constTable.GetConstant(e.index);
                            int val = int.Parse(sVal, CultureInfo.InvariantCulture);
                            _stack.Push(val);
                            pos++;
                        }
                        break;

                    case EEntryType.etCmdPtr:
                        {
                            _stack.Push(e.index);
                            pos++;
                        }
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
