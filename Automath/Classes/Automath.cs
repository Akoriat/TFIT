using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lab0.Classes
{
    public abstract class Automath
    {
        public TypeAutomaton Type { get; protected set; }
        protected string[] States;
        protected string[] Inputs;
        protected string[] FinalStates;
        protected string InitState;
        protected Dictionary<string, List<string>> Transitions;
        protected bool IsInitiatedCorrectly;

        protected Automath(TypeAutomaton type, string[] states, string[] inputs, string[] finalStates, string initState, Dictionary<string, List<string>> transitions)
        {
            Type = type;
            States = states;
            Inputs = inputs;
            FinalStates = finalStates;
            InitState = initState;
            Transitions = transitions;
            IsInitiatedCorrectly = true;
        }

        public static Automath CreateFromFile(string path)
        {
            using (StreamReader file = new StreamReader(path))
            {
                string parameter = file.ReadLine();
                TypeAutomaton type;
                if (parameter == "DKA")
                    type = TypeAutomaton.DKA;
                else if (parameter == "NKA")
                    type = TypeAutomaton.NKA;
                else if (parameter == "NKA-E")
                    type = TypeAutomaton.ENKA;
                else
                {
                    type = TypeAutomaton.None;
                    Console.WriteLine($"Введён некорректный тип автомата: '{parameter}'");
                    Console.ResetColor();
                    return null;
                }

                string line;
                string[] states = null;
                bool gotStates = false;
                string[] inputs = null;
                bool gotInputs = false;
                string initState = "";
                bool gotInitState = false;
                string[] finalStates = null;
                bool gotFinalStates = false;
                Dictionary<string, List<string>> transitions = new Dictionary<string, List<string>>();
                bool gotTransitions = false;
                bool emergencyStop = false;

                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("Q:"))
                    {
                        line = line.Replace("Q:", "").Replace(" ", "");
                        states = line.Split(',');
                        gotStates = true;
                    }
                    else if (line.StartsWith("S:"))
                    {
                        line = line.Replace("S:", "").Replace(" ", "");
                        inputs = line.Split(',');
                        gotInputs = true;
                    }
                    else if (line.StartsWith("Q0:"))
                    {
                        line = line.Replace("Q0:", "").Replace(" ", "");
                        initState = line;
                        gotInitState = true;
                    }
                    else if (line.StartsWith("F:"))
                    {
                        line = line.Replace("F:", "").Replace(" ", "");
                        finalStates = line.Split(',');
                        gotFinalStates = true;
                    }
                    else if (line.StartsWith("TT:") && gotStates && gotInputs)
                    {
                        for (int i = 0; i < states.Length; i++)
                        {
                            line = file.ReadLine();
                            if (line == null)
                            {
                                Console.WriteLine("Ошибка! Недостаточно строк в таблице переходов.");
                                Console.ResetColor();
                                emergencyStop = true;
                                break;
                            }

                            string[] tempStates = line.Split(' ');
                            List<string> transitionList = new List<string>();

                            foreach (string state in tempStates)
                            {
                                if (state.StartsWith("{") && state.EndsWith("}"))
                                {
                                    transitionList.Add(state.Trim('{', '}'));
                                }
                                else
                                {
                                    transitionList.Add(state);
                                }

                                if (!state.StartsWith("{") && !states.Contains(state) && state != "~")
                                {
                                    Console.WriteLine($"Ошибка! Состояние '{state}', указанное в таблице переходов, не определено!");
                                    Console.ResetColor();
                                    emergencyStop = true;
                                    break;
                                }
                            }

                            if (emergencyStop)
                                break;

                            transitions.Add(states[i], transitionList);
                        }
                        gotTransitions = true;
                    }
                }

                if (emergencyStop || !(gotStates && gotInputs && gotInitState && gotFinalStates && gotTransitions))
                {
                    Console.WriteLine("Автомат не может быть инициализирован из-за критической ошибки!");
                    Console.ResetColor();
                    return null;
                }

                Console.WriteLine($"Автомат считан из файла {path}");

                switch (type)
                {
                    case TypeAutomaton.DKA:
                        return new DkaAutomath(states, inputs, finalStates, initState, transitions);
                    case TypeAutomaton.NKA:
                        return new DnaAutomath(states, inputs, finalStates, initState, transitions);
                    case TypeAutomaton.ENKA:
                        return new NkaEpsilonAutomath(states, inputs, finalStates, initState, transitions);
                    default:
                        return null;
                }
            }
        }

        public void ShowInfo()
        {
            if (IsInitiatedCorrectly)
            {
                if (Type == TypeAutomaton.DKA)
                    Console.WriteLine("Детерминированный КА");
                else if (Type == TypeAutomaton.NKA)
                    Console.WriteLine("Недетерминированный КА");
                else if (Type == TypeAutomaton.ENKA)
                    Console.WriteLine("Недетерминированный КА с е-переходами");
                Console.ResetColor();

                Console.Write("Состояния: ");
                foreach (string word in States)
                {
                    Console.Write(word + " ");
                }
                Console.ResetColor();
                Console.WriteLine();

                Console.Write("Алфавит: ");
                foreach (string word in Inputs)
                {
                    Console.Write(word + " ");
                }
                Console.ResetColor();
                Console.WriteLine();

                Console.Write("Начальное состояние: ");
                Console.WriteLine(InitState);
                Console.ResetColor();

                Console.Write("Финальное(ые) состояние(я): ");
                foreach (string word in FinalStates)
                {
                    Console.Write(word + " ");
                }
                Console.ResetColor();
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Операция 'Show' не может быть выполнена: автомат не проинициализирован.");
                Console.ResetColor();
            }
        }

        private string CenterText(string text, int width)
        {
            if (text.Length >= width)
                return text;
            int totalPadding = width - text.Length;
            int padLeft = totalPadding / 2;
            int padRight = totalPadding - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        private string AlignState(string displayState, int width)
        {
            string prefix = "";
            string numberPart = displayState;
            if (displayState.StartsWith("->*"))
            {
                prefix = "->*";
                numberPart = displayState.Substring(3);
            }
            else if (displayState.StartsWith("->"))
            {
                prefix = "->";
                numberPart = displayState.Substring(2);
            }
            else if (displayState.StartsWith("*"))
            {
                prefix = "*";
                numberPart = displayState.Substring(1);
            }
            int remainingWidth = width - prefix.Length;
            string alignedNumber = numberPart.PadLeft(remainingWidth);
            return prefix + alignedNumber;
        }

        public void ShowTable()
        {
            if (!IsInitiatedCorrectly)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Операция 'ShowTable' не может быть выполнена: автомат не проинициализирован.");
                Console.ResetColor();
                return;
            }

            int cellWidth = Inputs.Concat(Transitions.Values.SelectMany(v => v)).Max(x => x.Length) + 2;
            int stateColWidth = cellWidth;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Таблица переходов автомата:");

            sb.Append(new string(' ', stateColWidth) + "|");
            foreach (var input in Inputs)
            {
                sb.Append(CenterText(input, cellWidth) + "|");
            }
            sb.AppendLine();

            int tableWidth = stateColWidth + 1 + (Inputs.Length * (cellWidth + 1));
            sb.AppendLine(new string('-', tableWidth));

            foreach (var state in States)
            {
                string displayState = state;
                if (state == InitState && FinalStates.Contains(state))
                    displayState = "->*" + state;
                else if (state == InitState)
                    displayState = "->" + state;
                else if (FinalStates.Contains(state))
                    displayState = "*" + state;

                sb.Append(AlignState(displayState, stateColWidth) + "|");
                foreach (var transition in Transitions[state])
                {
                    string cell = transition;
                    string trimmedCell = cell.Trim();
                    if (trimmedCell.Contains(",") && !(trimmedCell.StartsWith("{") && trimmedCell.EndsWith("}")))
                    {
                        cell = "{" + trimmedCell + "}";
                    }
                    sb.Append(CenterText(cell, cellWidth) + "|");
                }
                sb.AppendLine();
            }

            sb.AppendLine(new string('-', tableWidth));

            Console.WriteLine(sb.ToString());
        }




        protected int MaxLengthForTable()
        {
            int maxLength = 0;
            foreach (var item in Transitions.Values)
            {
                foreach (string state in item)
                {
                    if (maxLength < state.Length)
                        maxLength = state.Length;
                }
            }
            return maxLength;
        }

        public abstract bool ProcessInputLine(string word);
    }
}
