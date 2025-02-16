using System;
using System.Collections.Generic;
using System.Text;
using Lab4.Models;

namespace Lab4.Services
{
    /// <summary>
    /// Результат шага выполнения – для отображения в DataGrid.
    /// </summary>
    public class StepResult
    {
        public int Step { get; set; }
        public string Element { get; set; } = "";
        public string Stack { get; set; } = "";
        public string Variables { get; set; } = "";
        public int InstructionPointer { get; set; }
    }

    public class RuntimeValue
    {
        public int Value { get; set; }
        public bool IsAddress { get; set; }
    }

    public class StepInterpreter
    {
        private readonly List<PostfixEntry> _postfix;
        private readonly List<string> _varTable;
        private readonly List<string> _constTable;

        private readonly Dictionary<int, int> _variables = new Dictionary<int, int>();
        private readonly Stack<RuntimeValue> _stack = new Stack<RuntimeValue>();

        public StepInterpreter(List<PostfixEntry> postfix, IReadOnlyList<string> varTable, IReadOnlyList<string> constTable)
        {
            _postfix = postfix;
            _varTable = new List<string>(varTable);
            _constTable = new List<string>(constTable);
            for (int i = 0; i < _varTable.Count; i++)
            {
                _variables[i] = 0;
            }
        }

        public List<StepResult> ExecuteWithSteps()
        {
            List<StepResult> steps = new List<StepResult>();
            int ip = 0;
            int stepCount = 0;

            while (ip < _postfix.Count)
            {
                steps.Add(new StepResult
                {
                    Step = stepCount,
                    InstructionPointer = ip,
                    Element = DescribeEntry(_postfix[ip]),
                    Stack = StackToString(),
                    Variables = VariablesToString()
                });

                PostfixEntry entry = _postfix[ip];
                switch (entry.Type)
                {
                    case EEntryType.EtConst:
                        {
                            if (!int.TryParse(_constTable[entry.Index], out int constVal))
                                throw new Exception("Ошибка преобразования константы");
                            _stack.Push(new RuntimeValue { Value = constVal, IsAddress = false });
                            ip++;
                            break;
                        }
                    case EEntryType.EtVar:
                        {
                            _stack.Push(new RuntimeValue { Value = entry.Index, IsAddress = true });
                            ip++;
                            break;
                        }
                    case EEntryType.EtCmdPtr:
                        {
                            _stack.Push(new RuntimeValue { Value = entry.Index, IsAddress = false });
                            ip++;
                            break;
                        }
                    case EEntryType.EtCmd:
                        {
                            ECmd cmd = (ECmd)entry.Index;
                            switch (cmd)
                            {
                                case ECmd.ADD:
                                    {
                                        RuntimeValue op2 = _stack.Pop();
                                        RuntimeValue op1 = _stack.Pop();
                                        int res = GetOperandValue(op1) + GetOperandValue(op2);
                                        _stack.Push(new RuntimeValue { Value = res, IsAddress = false });
                                        break;
                                    }
                                case ECmd.SUB:
                                    {
                                        RuntimeValue op2 = _stack.Pop();
                                        RuntimeValue op1 = _stack.Pop();
                                        int res = GetOperandValue(op1) - GetOperandValue(op2);
                                        _stack.Push(new RuntimeValue { Value = res, IsAddress = false });
                                        break;
                                    }
                                case ECmd.MUL:
                                    {
                                        RuntimeValue op2 = _stack.Pop();
                                        RuntimeValue op1 = _stack.Pop();
                                        int res = GetOperandValue(op1) * GetOperandValue(op2);
                                        _stack.Push(new RuntimeValue { Value = res, IsAddress = false });
                                        break;
                                    }
                                case ECmd.DIV:
                                    {
                                        RuntimeValue op2 = _stack.Pop();
                                        RuntimeValue op1 = _stack.Pop();
                                        int divisor = GetOperandValue(op2);
                                        if (divisor == 0)
                                            throw new Exception("Деление на ноль");
                                        int res = GetOperandValue(op1) / divisor;
                                        _stack.Push(new RuntimeValue { Value = res, IsAddress = false });
                                        break;
                                    }
                                case ECmd.NOT:
                                    {
                                        RuntimeValue op = _stack.Pop();
                                        int res = (GetOperandValue(op) == 0) ? 1 : 0;
                                        _stack.Push(new RuntimeValue { Value = res, IsAddress = false });
                                        break;
                                    }
                                case ECmd.SET:
                                    {
                                        RuntimeValue rhs = _stack.Pop();
                                        RuntimeValue lhs = _stack.Pop();
                                        if (!lhs.IsAddress)
                                            throw new Exception("Ожидается адрес переменной для SET");
                                        int value = GetOperandValue(rhs);
                                        _variables[lhs.Value] = value;
                                        _stack.Push(new RuntimeValue { Value = value, IsAddress = false });
                                        break;
                                    }
                                case ECmd.OUT:
                                    {
                                        RuntimeValue op = _stack.Pop();
                                        int value = GetOperandValue(op);
                                        Console.WriteLine("OUT: " + value);
                                        break;
                                    }
                                case ECmd.JMP:
                                    {
                                        RuntimeValue ptr = _stack.Pop();
                                        ip = ptr.Value;
                                        continue;
                                    }
                                case ECmd.JZ:
                                    {
                                        RuntimeValue ptr = _stack.Pop();
                                        RuntimeValue cond = _stack.Pop();
                                        if (GetOperandValue(cond) == 0)
                                        {
                                            ip = ptr.Value;
                                            continue;
                                        }
                                        break;
                                    }
                                case ECmd.CMPE:
                                    {
                                        RuntimeValue op2 = _stack.Pop();
                                        RuntimeValue op1 = _stack.Pop();
                                        int res = (GetOperandValue(op1) == GetOperandValue(op2)) ? 0 : 1;
                                        _stack.Push(new RuntimeValue { Value = res, IsAddress = false });
                                        break;
                                    }
                                case ECmd.CMPNE:
                                    {
                                        RuntimeValue op2 = _stack.Pop();
                                        RuntimeValue op1 = _stack.Pop();
                                        int res = (GetOperandValue(op1) != GetOperandValue(op2)) ? 0 : 1;
                                        _stack.Push(new RuntimeValue { Value = res, IsAddress = false });
                                        break;
                                    }
                                case ECmd.CMPL:
                                    {
                                        RuntimeValue op2 = _stack.Pop();
                                        RuntimeValue op1 = _stack.Pop();
                                        int res = (GetOperandValue(op1) < GetOperandValue(op2)) ? 0 : 1;
                                        _stack.Push(new RuntimeValue { Value = res, IsAddress = false });
                                        break;
                                    }
                                case ECmd.CMPG:
                                    {
                                        RuntimeValue op2 = _stack.Pop();
                                        RuntimeValue op1 = _stack.Pop();
                                        int res = (GetOperandValue(op1) > GetOperandValue(op2)) ? 0 : 1;
                                        _stack.Push(new RuntimeValue { Value = res, IsAddress = false });
                                        break;
                                    }
                                default:
                                    throw new Exception("Неизвестная команда: " + cmd);
                            }
                            ip++;
                            break;
                        }
                    default:
                        throw new Exception("Неизвестный тип записи в ПОЛИЗе");
                }
                stepCount++;
            }
            return steps;
        }

        private int GetOperandValue(RuntimeValue op)
        {
            return op.IsAddress ? _variables[op.Value] : op.Value;
        }

        private string DescribeEntry(PostfixEntry entry)
        {
            return entry.Type switch
            {
                EEntryType.EtConst => "Const(" + _constTable[entry.Index] + ")",
                EEntryType.EtVar => "Var(" + _varTable[entry.Index] + ")",
                EEntryType.EtCmd => ((ECmd)entry.Index).ToString(),
                EEntryType.EtCmdPtr => "Ptr(" + entry.Index + ")",
                _ => "Unknown"
            };
        }

        private string StackToString()
        {
            List<string> list = new List<string>();
            foreach (var item in _stack)
            {
                if (item.IsAddress)
                    list.Add("Var[" + item.Value + "]=" + _variables[item.Value]);
                else
                    list.Add(item.Value.ToString());
            }
            return string.Join(" | ", list);
        }

        private string VariablesToString()
        {
            List<string> list = new List<string>();
            foreach (var kvp in _variables)
            {
                list.Add(_varTable[kvp.Key] + "=" + kvp.Value);
            }
            return string.Join(" ; ", list);
        }
    }
}
