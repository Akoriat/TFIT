using System;
using System.Collections.Generic;
using System.Text;
using Lab4.Models;


namespace Lab4.Services
{
    public static class Interpreter
    {
        private static List<PostfixEntry> Postfix;
        private static List<string> Identifiers;
        private static List<string> Constants;
        private static int pos;
        private static List<StackElement> stack;
        private static int[] variables;
        private static StringBuilder output;
        private struct StackElement
        {
            public bool IsVar;   
            public int VarIndex;
            public int Value;
        }

        private static int GetValue(StackElement se)
        {
            return se.IsVar ? variables[se.VarIndex] : se.Value;
        }

        private static void Push(StackElement se)
        {
            stack.Add(se);
        }

        private static StackElement Pop()
        {
            if (stack.Count == 0)
                throw new Exception("Ошибка: переполнение стека");
            var se = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            return se;
        }

        private static void PushElm(PostfixEntry entry)
        {
            var se = new StackElement();
            switch (entry.Type)
            {
                case EEntryType.etConst:
                    se.IsVar = false;
                    se.Value = int.Parse(Constants[entry.Index]);
                    break;
                case EEntryType.etVar:
                    se.IsVar = true;
                    se.VarIndex = entry.Index;
                    se.Value = variables[entry.Index];
                    break;
                case EEntryType.etCmdPtr:
                    se.IsVar = false;
                    se.Value = entry.Index;
                    break;
            }
            Push(se);
        }

        private static int PopVal()
        {
            return GetValue(Pop());
        }

        private static void SetVarAndPop(int val)
        {
            var se = Pop();
            if (!se.IsVar)
                throw new Exception("SET: Ожидалась ссылка на переменную.");
            variables[se.VarIndex] = val;
        }

        public static string InterpretWithLogging(List<PostfixEntry> postfix, List<string> idTable, List<string> constTable)
        {
            Postfix = postfix;
            Identifiers = idTable;
            Constants = constTable;
            pos = 0;
            stack = new List<StackElement>();
            variables = new int[Identifiers.Count];
            output = new StringBuilder();
            var log = new StringBuilder();

            log.AppendLine("=== Начало интерпретации ===");
            while (pos < Postfix.Count)
            {
                log.AppendLine($"Инструкция {pos}: {Postfix[pos]}");
                log.AppendLine($"Состояние стека до выполнения: {GetStackState()}");
                var prevPos = pos;
                if (Postfix[pos].Type == EEntryType.etCmd)
                {
                    var cmd = (ECmd)Postfix[pos].Index;
                    switch (cmd)
                    {
                        case ECmd.JMP:
                            {
                                var jumpAddr = PopVal();
                                log.AppendLine($"Выполняется команда JMP, переход на адрес {jumpAddr}");
                                pos = jumpAddr;
                            }
                            break;
                        case ECmd.JZ:
                            {
                                var addr = PopVal();
                                var cond = PopVal();
                                log.AppendLine($"Выполняется команда JZ: условие = {cond}, адрес = {addr}");
                                if (cond != 0)
                                {
                                    pos++;
                                    log.AppendLine("Условие истинно, переходим к следующей инструкции.");
                                }
                                else
                                {
                                    pos = addr;
                                    log.AppendLine("Условие ложно, переход по адресу.");
                                }
                            }
                            break;
                        case ECmd.SET:
                            {
                                var value = PopVal();
                                SetVarAndPop(value);
                                log.AppendLine($"Выполняется SET, присваиваем значение {value}");
                                pos++;
                            }
                            break;
                        case ECmd.ADD:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                var res = a + b;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"ADD: {a} + {b} = {res}");
                                pos++;
                            }
                            break;
                        case ECmd.SUB:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                var res = a - b;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"SUB: {a} - {b} = {res}");
                                pos++;
                            }
                            break;
                        case ECmd.MUL:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                var res = a * b;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"MUL: {a} * {b} = {res}");
                                pos++;
                            }
                            break;
                        case ECmd.DIV:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                if (b == 0)
                                    throw new Exception("Деление на ноль!");
                                var res = a / b;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"DIV: {a} / {b} = {res}");
                                pos++;
                            }
                            break;
                        case ECmd.AND:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                var res = (a != 0 && b != 0) ? 1 : 0;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"AND: {a} && {b} = {res}");
                                pos++;
                            }
                            break;
                        case ECmd.OR:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                var res = (a != 0 || b != 0) ? 1 : 0;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"OR: {a} || {b} = {res}");
                                pos++;
                            }
                            break;
                        case ECmd.NOT:
                            {
                                var a = PopVal();
                                var res = (a == 0) ? 1 : 0;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"NOT: !{a} = {res}");
                                pos++;
                            }
                            break;
                        case ECmd.CMPE:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                var res = (a == b) ? 1 : 0;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"CMPE: {a} == {b} ? {res}");
                                pos++;
                            }
                            break;
                        case ECmd.CMPNE:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                var res = (a != b) ? 1 : 0;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"CMPNE: {a} <> {b} ? {res}");
                                pos++;
                            }
                            break;
                        case ECmd.CMPL:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                var res = (a < b) ? 1 : 0;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"CMPL: {a} < {b} ? {res}");
                                pos++;
                            }
                            break;
                        case ECmd.CMPLE:
                            {
                                var b = PopVal();
                                var a = PopVal();
                                var res = (a <= b) ? 1 : 0;
                                Push(new StackElement { IsVar = false, Value = res });
                                log.AppendLine($"CMPLE: {a} <= {b} ? {res}");
                                pos++;
                            }
                            break;
                        case ECmd.OUTPUT:
                            {
                                var val = PopVal();
                                output.AppendLine(val.ToString());
                                log.AppendLine($"OUTPUT: выведено значение {val}");
                                pos++;
                            }
                            break;
                        default:
                            throw new Exception("Неизвестная команда: " + cmd.ToString());
                    }
                }
                else
                {
                    PushElm(Postfix[pos]);
                    log.AppendLine($"Помещён операнд: {Postfix[pos]}");
                    pos++;
                }
                log.AppendLine($"Состояние стека после выполнения: {GetStackState()}");
                log.AppendLine($"Текущее состояние переменных: {GetVariablesState()}");
                log.AppendLine(new string('-', 40));
            }
            log.AppendLine("=== Конец интерпретации ===");
            output.AppendLine("\n=== Лог интерпретации ===");
            output.AppendLine(log.ToString());
            return output.ToString();
        }

        private static string GetStackState()
        {
            if (stack.Count == 0)
                return "<пусто>";
            var sb = new StringBuilder();
            foreach (var se in stack)
            {
                sb.Append(GetValue(se) + " ");
            }
            return sb.ToString();
        }

        private static string GetVariablesState()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Identifiers.Count; i++)
            {
                sb.Append($"{Identifiers[i]}={variables[i]} ");
            }
            return sb.ToString();
        }
    }
}
