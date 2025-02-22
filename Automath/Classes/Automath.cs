﻿using System;
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
            using (var file = new StreamReader(path))
            {
                var parameter = file.ReadLine();
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
                    return null;
                }

                string line;
                string[] states = [];
                var gotStates = false;
                string[] inputs = [];
                var gotInputs = false;
                var initState = "";
                var gotInitState = false;
                string[] finalStates = [];
                var gotFinalStates = false;
                Dictionary<string, List<string>> transitions = [];
                var gotTransitions = false;
                var emergencyStop = false;

                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("Q:"))
                    {
                        line = line.Replace("Q:", "").Replace(" ", "");
                        states = line.Split(',');
                        if(states.Length == 0)
                        {
                            Console.WriteLine("Введите хотя бы одно состояние.");
                            emergencyStop = true;
                            break;
                        }
                        gotStates = true;
                    }
                    else if (line.StartsWith("S:"))
                    {
                        line = line.Replace("S:", "").Replace(" ", "");
                        inputs = line.Split(',');
                        if (inputs.Length == 0)
                        {
                            Console.WriteLine("Введите хотя бы один символ алфавита.");
                            emergencyStop = true;
                            break;
                        }
                        if (type == TypeAutomaton.ENKA && !inputs.Contains("ε"))
                        {
                            Console.WriteLine("Для ЕНКА должно быть задано состояние ε.");
                            emergencyStop = true;
                            break;
                        }
                        gotInputs = true;
                    }
                    else if (line.StartsWith("Q0:"))
                    {
                        line = line.Replace("Q0:", "").Replace(" ", "");
                        initState = line;
                        if (initState.Length == 0)
                        {
                            Console.WriteLine("Введите начальное состояние.");
                            emergencyStop = true;
                            break;
                        }
                        if (initState.Split(", ").Length > 1 && type == TypeAutomaton.DKA)
                        {
                            Console.WriteLine("Для ДКА нельзя задать больше одного начального состояния");
                            emergencyStop = true;
                            break;
                        }
                        gotInitState = true;
                    }
                    else if (line.StartsWith("F:"))
                    {
                        line = line.Replace("F:", "").Replace(" ", "");
                        finalStates = line.Split(',');
                        if (finalStates.Length == 0)
                        {
                            Console.WriteLine("Введите хотя бы одно финальное состояние.");
                            emergencyStop = true;
                            break;
                        }
                        gotFinalStates = true;
                    }
                    else if (line.StartsWith("TT:") && gotStates && gotInputs)
                    {
                        for (var i = 0; i < states.Length; i++)
                        {
                            line = file.ReadLine();
                            if (line == null)
                            {
                                Console.WriteLine("Ошибка! Недостаточно строк в таблице переходов.");
                                emergencyStop = true;
                                break;
                            }

                            string[] tempStates = line.Split(' ');
                            List<string> transitionList = new List<string>();

                            foreach (var state in tempStates)
                            {
                                if (state.StartsWith("{") && state.EndsWith("}"))
                                {
                                    if (type == TypeAutomaton.DKA)
                                    {
                                        Console.WriteLine("Для ДКА нельзя задавать множественные состояния.");
                                        emergencyStop = true;
                                        break;
                                    }
                                    else
                                    {
                                        transitionList.Add(state);
                                    }
                                }
                                else
                                {
                                    transitionList.Add(state);
                                }

                                if (!state.StartsWith("{") && !states.Contains(state) && state != "~")
                                {
                                    Console.WriteLine($"Ошибка! Состояние '{state}', указанное в таблице переходов, не определено!");
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

                Console.Write("Состояния: ");
                foreach (var word in States)
                {
                    Console.Write(word + " ");
                }
                Console.WriteLine();

                Console.Write("Алфавит: ");
                foreach (var word in Inputs)
                {
                    Console.Write(word + " ");
                }
                Console.WriteLine();

                Console.Write("Начальное состояние: ");
                Console.WriteLine(InitState);

                Console.Write("Финальное(ые) состояние(я): ");
                foreach (var word in FinalStates)
                {
                    Console.Write(word + " ");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Операция 'Show' не может быть выполнена: автомат не проинициализирован.");
            }
        }

        private static string CenterText(string text, int width)
        {
            if (text.Length >= width)
                return text;
            var totalPadding = width - text.Length;
            var padLeft = totalPadding / 2;
            var padRight = totalPadding - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        private static string AlignState(string displayState, int width)
        {
            var prefix = "";
            var numberPart = displayState;
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
            var remainingWidth = width - prefix.Length;
            var alignedNumber = numberPart.PadLeft(remainingWidth);
            return prefix + alignedNumber;
        }

        public void ShowTable()
        {
            if (!IsInitiatedCorrectly)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Операция 'ShowTable' не может быть выполнена: автомат не проинициализирован.");
                return;
            }

            var cellWidth = Inputs.Concat(Transitions.Values.SelectMany(v => v)).Max(x => x.Length) + 2;
            var stateColWidth = cellWidth;

            var sb = new StringBuilder();
            sb.AppendLine("Таблица переходов автомата:");

            sb.Append(new string(' ', stateColWidth) + "|");
            foreach (var input in Inputs)
            {
                sb.Append(CenterText(input, cellWidth) + "|");
            }
            sb.AppendLine();

            var tableWidth = stateColWidth + 1 + (Inputs.Length * (cellWidth + 1));
            sb.AppendLine(new string('-', tableWidth));

            foreach (var state in States)
            {
                var displayState = state;
                if (state == InitState && FinalStates.Contains(state))
                    displayState = "->*" + state;
                else if (state == InitState)
                    displayState = "->" + state;
                else if (FinalStates.Contains(state))
                    displayState = "*" + state;

                sb.Append(AlignState(displayState, stateColWidth) + "|");
                foreach (var transition in Transitions[state])
                {
                    var cell = transition;
                    var trimmedCell = cell.Trim();
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
            var maxLength = 0;
            foreach (var item in Transitions.Values)
            {
                foreach (var state in item)
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
