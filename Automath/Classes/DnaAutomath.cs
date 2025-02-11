using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab0.Classes
{
    public class DnaAutomath : Automath
    {
        public DnaAutomath(string[] states, string[] inputs, string[] finalStates, string initState, Dictionary<string, List<string>> transitions)
            : base(TypeAutomaton.NKA, states, inputs, finalStates, initState, transitions)
        {
        }

        public override bool ProcessInputLine(string word)
        {
            if (!IsInitiatedCorrectly)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Операция 'ProcessInputLine' не может быть выполнена: автомат не проинициализирован.");
                Console.ResetColor();
                return false;
            }
            bool emergencyBreak = false;
            List<string> inputsList = Inputs.ToList();
            List<string> reachableStates = new List<string>();
            List<string> currentStates = new List<string>() { InitState };

            Console.WriteLine($"\nТекущее состояние: {InitState}");

            foreach (char symbol in word)
            {
                if (Inputs.Contains(symbol.ToString()))
                {
                    Console.WriteLine($"Считан символ '{symbol}'");
                    foreach (string item in currentStates)
                    {
                        string tempState = Transitions[item][inputsList.IndexOf(symbol.ToString())];
                        if (tempState.Contains("{"))
                        {
                            tempState = tempState.Trim('{', '}');
                            foreach (string state in tempState.Split(','))
                            {
                                if (!reachableStates.Contains(state) && state != "~")
                                    reachableStates.Add(state);
                            }
                        }
                        else
                        {
                            if (tempState == "~" && currentStates.Count == 1)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Запрашиваемое входным символом состояние не определено.\nИз состояния {currentStates[0]} нет перехода по символу {symbol}");
                                Console.ResetColor();
                                emergencyBreak = true;
                                return false;
                            }
                            if (!reachableStates.Contains(tempState) && tempState != "~")
                                reachableStates.Add(tempState);
                        }
                    }

                    if (reachableStates.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Запрашиваемое входным символом состояние не определено.\nИз состояний нет перехода по символу {symbol}");
                        Console.ResetColor();
                        emergencyBreak = true;
                        return false;
                    }

                    currentStates = new List<string>(reachableStates);
                    reachableStates.Clear();
                    Console.Write(" - Текущее(ие) состояние(ия): ");
                    foreach (string item in currentStates)
                    {
                        Console.Write($"'{item}', ");
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка! Считанный символ '{symbol}' не входит в алфавит!");
                    Console.ResetColor();
                    emergencyBreak = true;
                    break;
                }
            }
            if (!emergencyBreak)
            {
                if (currentStates.Any(state => FinalStates.Contains(state)))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Одно из состояний ");
                    foreach (string item in currentStates)
                    {
                        Console.Write($"'{item}', ");
                    }
                    Console.WriteLine("входит в число финальных состояний.");
                    Console.ResetColor();
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Ни одно из состояний ");
                    foreach (string item in currentStates)
                    {
                        Console.Write($"'{item}', ");
                    }
                    Console.WriteLine("не входит в число финальных состояний.");
                    Console.ResetColor();
                }
            }
            return false;
        }

        public DkaAutomath ToDka()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nВыполняется преобразование НКА к ДКА:\n");
            Console.ResetColor();

            List<string> newStates = new List<string>() { InitState };
            List<string> newFinalStates = new List<string>();
            Dictionary<string, List<string>> newTransitions = new Dictionary<string, List<string>>();
            bool run = true;
            int i = 0;

            while (run)
            {
                string currentState = newStates[i];
                if (!currentState.Contains("{"))
                {
                    foreach (string item in Transitions[currentState])
                    {
                        if (!newStates.Contains(item))
                        {
                            newStates.Add(item);
                            string[] tempStates = item.Trim('{', '}').Split(',');
                            bool isFinal = tempStates.Any(state => FinalStates.Contains(state));
                            if (isFinal && !newFinalStates.Contains(item))
                                newFinalStates.Add(item);
                        }
                    }
                    newTransitions.Add(currentState, Transitions[currentState]);
                }
                else
                {
                    List<string> results = new List<string>();
                    string tempStates = currentState.Trim('{', '}');
                    List<HashSet<string>> reachableFromInputs = new List<HashSet<string>>();
                    for (int j = 0; j < Inputs.Length; j++)
                    {
                        reachableFromInputs.Add(new HashSet<string>());
                    }
                    foreach (string state in tempStates.Split(','))
                    {
                        for (int j = 0; j < Inputs.Length; j++)
                        {
                            string[] splitted = Transitions[state][j].Trim('{', '}').Split(',');
                            foreach (string symbol in splitted)
                            {
                                reachableFromInputs[j].Add(symbol);
                            }
                        }
                    }
                    foreach (var item in reachableFromInputs)
                    {
                        string[] statesArray = item.ToArray();
                        Array.Sort(statesArray);
                        bool isFinal = statesArray.Any(s => FinalStates.Contains(s));
                        if (statesArray.Length > 1 || (statesArray.Length == 1 && statesArray[0] != "~"))
                        {
                            string result = "{" + string.Join(",", statesArray.Where(s => s != "~")) + "}";
                            if (isFinal && !newFinalStates.Contains(result))
                                newFinalStates.Add(result);
                            results.Add(result);
                            if (!newStates.Contains(result))
                                newStates.Add(result);
                        }
                        else
                        {
                            if (statesArray.Length > 0)
                            {
                                if (FinalStates.Contains(statesArray[0]) && !newFinalStates.Contains(statesArray[0]))
                                    newFinalStates.Add(statesArray[0]);
                                results.Add(statesArray[0]);
                                if (!newStates.Contains(statesArray[0]))
                                    newStates.Add(statesArray[0]);
                            }
                        }
                    }
                    newTransitions.Add(currentState, new List<string>(results));
                }

                i++;
                if (i >= newStates.Count)
                    run = false;
                else
                {
                    while (i < newStates.Count && newStates[i] == "~")
                    {
                        i++;
                        if (i >= newStates.Count)
                        {
                            run = false;
                            break;
                        }
                    }
                }
            }

            newStates.Remove("~");
            return new DkaAutomath(newStates.ToArray(), Inputs, newFinalStates.Distinct().ToArray(), InitState, newTransitions);
        }
    }
}
