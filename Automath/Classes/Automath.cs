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
                            if (line == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Ошибка! Недостаточно строк в таблице переходов.");
                                Console.ResetColor();
                                emergencyStop = true;
                                break;
                            }

                            // Обработка множественных состояний (например, {2,3})
                            string[] tempStates = line.Split(' ');
                            List<string> transitionList = new List<string>();

                            foreach (string state in tempStates)
                            {
                                if (state.StartsWith("{") && state.EndsWith("}"))
                                {
                                    // Убираем фигурные скобки и добавляем как одно состояние
                                    transitionList.Add(state.Trim('{', '}'));
                                }
                                else
                                {
                                    // Одиночное состояние
                                    transitionList.Add(state);
                                }

                                // Проверка, что состояние существует
                                if (!state.StartsWith("{") && !states.Contains(state) && state != "~")
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
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

                // Вывод заголовка таблицы
                for (int j = 0; j < Inputs.Length + 1; j++)
                {
                    if (j == 0)
                        Console.Write("{0,-" + (maxLength + 2) + "}\t", "");
                    else
                        Console.Write("|{0,-" + (maxLength + 1) + "}", Inputs[j - 1]);
                }
                Console.WriteLine();

                // Вывод строк таблицы
                for (int i = 0; i < States.Length; i++)
                {
                    for (int j = 0; j < Inputs.Length + 1; j++)
                    {
                        if (j == 0)
                        {
                            // Вывод состояния с маркерами начального и финального состояний
                            if (States[i] == InitState && FinalStates.Contains(States[i]))
                                Console.Write("->*{0,-" + (maxLength)     + "}: \t", States[i]);
                            else if (States[i] == InitState)
                                Console.Write("->{0,-" + (maxLength + 1) + "}: \t", States[i]);
                            else if (FinalStates.Contains(States[i]))
                                Console.Write("*{0,-" + (maxLength + 1) + "}: \t", States[i]);
                            else
                                Console.Write("{0,-" + (maxLength + 1) + "}: \t", States[i]);
                        }
                        else
                        {
                            // Вывод переходов
                            string transition = Transitions[States[i]][j - 1];
                            if (transition.StartsWith("{") && transition.EndsWith("}"))
                            {
                                // Убираем фигурные скобки для красивого вывода
                                transition = transition.Trim('{', '}');
                            }
                            Console.Write("|{0,-" + (maxLength + 1) + "}", transition);
                        }
                    }
                    Console.WriteLine();
                }

                // Вывод информации о ε-переходах (если это НКА с ε-переходами)
                if (Type == TypeAutomaton.ENKA)
                {
                    Console.WriteLine("\nПримечание: Последний столбец таблицы соответствует ε-переходам.");
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
