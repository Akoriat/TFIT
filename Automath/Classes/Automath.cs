using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                    Console.ForegroundColor = ConsoleColor.Red;
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
                            string[] tempStates = line.Split(' ');
                            foreach (string state in tempStates)
                            {
                                if (!states.ToList().Contains(state))
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Ошибка! Состояние '{state}', указанное в таблице переходов, не определено!");
                                    Console.ResetColor();
                                    emergencyStop = true;
                                    break;
                                }
                            }
                            transitions.Add(states[i], tempStates.ToList());
                        }
                        gotTransitions = true;
                    }
                }

                if (emergencyStop || !(gotStates && gotInputs && gotInitState && gotFinalStates && gotTransitions))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
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
                        return new DnaEpsilonAutomath(states, inputs, finalStates, initState, transitions);
                    default:
                        return null;
                }
            }
        }

        public void ShowInfo()
        {
            if (IsInitiatedCorrectly)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (Type == TypeAutomaton.DKA)
                    Console.WriteLine("Детерминированный КА");
                else if (Type == TypeAutomaton.NKA)
                    Console.WriteLine("Недетерминированный КА");
                else if (Type == TypeAutomaton.ENKA)
                    Console.WriteLine("Недетерминированный КА с е-переходами");
                Console.ResetColor();

                Console.Write("Состояния: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (string word in States)
                {
                    Console.Write(word + " ");
                }
                Console.ResetColor();
                Console.WriteLine();

                Console.Write("Алфавит: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (string word in Inputs)
                {
                    Console.Write(word + " ");
                }
                Console.ResetColor();
                Console.WriteLine();

                Console.Write("Начальное состояние: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(InitState);
                Console.ResetColor();

                Console.Write("Финальное(ые) состояние(я): ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (string word in FinalStates)
                {
                    Console.Write(word + " ");
                }
                Console.ResetColor();
                Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Операция 'Show' не может быть выполнена: автомат не проинициализирован.");
                Console.ResetColor();
            }
        }

        public void ShowTable()
        {
            if (IsInitiatedCorrectly)
            {
                int maxLength = MaxLengthForTable();

                Console.WriteLine("Таблица переходов автомата:");

                for (int i = 0; i < States.Length + 1; i++)
                {
                    if (i == 0)
                    {
                        for (int j = 0; j < Inputs.Length + 1; j++)
                        {
                            if (j == 0)
                                Console.Write("{0,-" + (maxLength + 2) + "}\t", "");
                            else
                                Console.Write("|{0,-" + (maxLength + 1) + "}", Inputs[j - 1]);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < Inputs.Length + 1; j++)
                        {
                            if (j == 0)
                            {
                                if (States[i - 1] == InitState && FinalStates.Contains(States[i - 1]))
                                    Console.Write("->*{0,-" + (maxLength) + "}: \t", States[i - 1]);
                                else if (States[i - 1] == InitState)
                                    Console.Write("->{0,-" + (maxLength + 1) + "}: \t", States[i - 1]);
                                else if (FinalStates.Contains(States[i - 1]))
                                    Console.Write(" *{0,-" + (maxLength + 1) + "}: \t", States[i - 1]);
                                else
                                    Console.Write("  {0,-" + (maxLength + 1) + "}: \t", States[i - 1]);
                            }
                            else
                                Console.Write("|{0,-" + (maxLength + 1) + "}", Transitions[States[i - 1]][j - 1]);
                        }
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Операция 'ShowTable' не может быть выполнена: автомат не проинициализирован.");
                Console.ResetColor();
            }
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
