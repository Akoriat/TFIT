using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab0.Classes
{
    public class DnaEpsilonAutomath : Automath
    {
        public DnaEpsilonAutomath(string[] states, string[] inputs, string[] finalStates, string initState, Dictionary<string, List<string>> transitions)
            : base(TypeAutomaton.ENKA, states, inputs, finalStates, initState, transitions)
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
            bool deadEnd = false;
            bool epsCycle = false;
            List<string> inputsList = Inputs.ToList();
            List<string> reachableStates = new List<string>();
            List<string> epsClosure = new List<string>();
            List<string> currentStates = new List<string>() { InitState };
            int tick = 0;
            Dictionary<string, List<string>> allClosures = GetAllClosures();

            Console.WriteLine($"\nТекущее состояние: {InitState}");

            foreach (char symbol in word)
            {
                tick++;
                if (Inputs.Contains(symbol.ToString()))
                {
                    Console.WriteLine($"\nТакт №{tick}. Считан символ '{symbol}'");
                    foreach (string item in currentStates)
                    {
                        string tempState = Transitions[item][inputsList.IndexOf(symbol.ToString())];
                        if (tempState.Contains("{"))
                        {
                            tempState = tempState.Trim('{', '}');
                            foreach (string state in tempState.Split(','))
                            {
                                if (!reachableStates.Contains(state))
                                    reachableStates.Add(state);
                            }
                        }
                        else
                        {
                            if (!reachableStates.Contains(tempState))
                                reachableStates.Add(tempState);
                        }
                    }

                    if (reachableStates.Count != 1)
                        reachableStates.Remove("~");
                    currentStates = new List<string>(reachableStates);
                    reachableStates.Clear();

                    Console.Write(" - Текущее(ие) состояние(ия): ");
                    currentStates.Sort();
                    foreach (string item in currentStates)
                    {
                        Console.Write($"'{item}', ");
                    }
                    Console.WriteLine();

                    List<string> prevCurrent = new List<string>(currentStates);

                    if (currentStates.Count == 1 && currentStates[0] == "~")
                    {
                        deadEnd = true;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Из текущего состояния отсутствуют переходы в другие состояния.");
                        Console.ResetColor();
                        break;
                    }

                    int i = 0;
                    while (i < currentStates.Count)
                    {
                        string toAdd = "";
                        string item = currentStates[i];
                        if (Transitions[item][Inputs.Length - 1] != "~")
                        {
                            epsClosure.Add(item);
                            toAdd = Transitions[item][Inputs.Length - 1];
                            if (!epsClosure.Contains(toAdd))
                                epsClosure.Add(toAdd);
                            else
                            {
                                epsCycle = true;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(toAdd))
                        {
                            if (!toAdd.Contains('{'))
                            {
                                if (!currentStates.Contains(toAdd))
                                    currentStates.Add(toAdd);
                            }
                            else
                            {
                                string tempState = toAdd.Trim('{', '}');
                                foreach (string state in tempState.Split(','))
                                {
                                    if (!currentStates.Contains(state))
                                        currentStates.Add(state);
                                }
                            }
                        }
                        i++;
                    }

                    if (epsCycle)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Обнаружен цикл по эпсилон-переходам. Программа будет остановлена.");
                        epsClosure = epsClosure.Distinct().ToList();
                        foreach (var s in epsClosure)
                        {
                            Console.Write($"{s} -> ");
                        }
                        Console.WriteLine("...");
                        Console.ResetColor();
                        break;
                    }

                    bool epsAdded = false;
                    foreach (string item in epsClosure)
                    {
                        if (!item.Contains('{'))
                        {
                            if (!currentStates.Contains(item))
                                currentStates.Add(item);
                            epsAdded = true;
                        }
                        else
                        {
                            string tempState = item.Trim('{', '}');
                            foreach (string state in tempState.Split(','))
                            {
                                if (!currentStates.Contains(state))
                                    currentStates.Add(state);
                            }
                            epsAdded = true;
                        }
                    }

                    if (epsAdded)
                    {
                        foreach (string item in prevCurrent)
                        {
                            Console.Write($"Состояние {item} образует следующее замыкание: ");
                            foreach (string state in allClosures[item])
                            {
                                Console.Write($"{state}, ");
                            }
                            Console.WriteLine();
                        }
                    }
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

            if (!emergencyBreak && !deadEnd && !epsCycle)
            {
                if (currentStates.Any(state => FinalStates.Contains(state)))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Одно из достигнутых состояний ");
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
            else if (!emergencyBreak && deadEnd)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Чтение строки невозможно продолжить");
                Console.ResetColor();
            }
            return false;
        }

        public DnaAutomath ToNka()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nВыполняется преобразование НКА с эпсилон-переходами к 'обычному' НКА:\n");
            Console.ResetColor();

            List<string> newInputs = Inputs.ToList();
            newInputs.RemoveAt(Inputs.Length - 1);
            List<string> newStates = new List<string>();
            List<string> newFinalStates = new List<string>();
            Dictionary<string, List<string>> newTransitions = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> closures = GetAllClosures();

            int amountOfInputs = Inputs.Length - 1;
            for (int i = 0; i < States.Length; i++)
            {
                string state = States[i];
                string[] tempForEachInput = new string[amountOfInputs];
                if (Transitions[state][Inputs.Length - 1] == "~")
                {
                    newStates.Add(state);
                    Transitions[state].RemoveAt(amountOfInputs);
                    newTransitions.Add(state, Transitions[state]);
                    if (FinalStates.Contains(state))
                    {
                        newFinalStates.Add(state);
                    }
                }
                else
                {
                    newStates.Add(state);
                    List<string> tempTransitions = new List<string>();
                    foreach (string item in closures[state])
                    {
                        for (int j = 0; j < amountOfInputs; j++)
                        {
                            if (Transitions[item][j] != "~")
                            {
                                tempForEachInput[j] += Transitions[item][j] + ",";
                            }
                        }
                        if (FinalStates.Contains(item))
                        {
                            newFinalStates.Add(state);
                        }
                    }
                    foreach (string item in tempForEachInput)
                    {
                        if (string.IsNullOrEmpty(item))
                        {
                            tempTransitions.Add("~");
                        }
                        else
                        {
                            string[] tempStates = item.TrimEnd(',').Split(',');
                            if (tempStates.Length == 1)
                            {
                                tempTransitions.Add(tempStates[0]);
                            }
                            else
                            {
                                string result = "{" + string.Join(",", tempStates) + "}";
                                tempTransitions.Add(result);
                            }
                        }
                    }
                    newTransitions.Add(state, tempTransitions);
                }
            }
            return new DnaAutomath(newStates.ToArray(), newInputs.ToArray(), newFinalStates.ToArray(), InitState, newTransitions);
        }

        public Dictionary<string, List<string>> GetAllClosures()
        {
            if (Type == TypeAutomaton.ENKA)
            {
                Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
                foreach (string state in Transitions.Keys)
                {
                    List<string> epsClosure = new List<string>();
                    List<string> statesInProcess = new List<string>() { state };
                    int i = 0;
                    while (i < statesInProcess.Count)
                    {
                        string current = statesInProcess[i];
                        if (Transitions[current][Inputs.Length - 1] != "~")
                        {
                            if (!epsClosure.Contains(current))
                                epsClosure.Add(current);
                            string toAdd = Transitions[current][Inputs.Length - 1];
                            if (!epsClosure.Contains(toAdd))
                                epsClosure.Add(toAdd);
                            if (!statesInProcess.Contains(toAdd))
                                statesInProcess.Add(toAdd);
                        }
                        i++;
                    }
                    result.Add(state, epsClosure.Distinct().ToList());
                }
                return result;
            }
            return null;
        }
    }
}
