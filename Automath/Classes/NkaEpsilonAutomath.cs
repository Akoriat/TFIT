﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab0.Classes
{
    public class NkaEpsilonAutomath : Automath
    {
        public NkaEpsilonAutomath(string[] states, string[] inputs, string[] finalStates, string initState, Dictionary<string, List<string>> transitions)
            : base(TypeAutomaton.ENKA, states, inputs, finalStates, initState, transitions)
        {
        }

        public override bool ProcessInputLine(string word)
        {
            if (!IsInitiatedCorrectly)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Операция 'ProcessInputLine' не может быть выполнена: автомат не проинициализирован.");
                return false;
            }
            var emergencyBreak = false;
            var deadEnd = false;
            var epsCycle = false;
            List<string> inputsList = Inputs.ToList();
            List<string> reachableStates = new List<string>();
            List<string> epsClosure = new List<string>();
            List<string> currentStates = new List<string>() { InitState };
            var tick = 0;
            Dictionary<string, List<string>> allClosures = GetAllClosures();

            Console.WriteLine($"\nТекущее состояние: {InitState}");

            foreach (var symbol in word)
            {
                tick++;
                if (Inputs.Contains(symbol.ToString()))
                {
                    Console.WriteLine($"\nТакт №{tick}. Считан символ '{symbol}'");
                    foreach (var item in currentStates)
                    {
                        var tempState = Transitions[item][inputsList.IndexOf(symbol.ToString())];
                        if (tempState.Contains("{"))
                        {
                            tempState = tempState.Trim('{', '}');
                            foreach (var state in tempState.Split(','))
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
                    foreach (var item in currentStates)
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
                        break;
                    }

                    var i = 0;
                    while (i < currentStates.Count)
                    {
                        var toAdd = "";
                        var item = currentStates[i];
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
                                var tempState = toAdd.Trim('{', '}');
                                foreach (var state in tempState.Split(','))
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

                        break;
                    }

                    var epsAdded = false;
                    foreach (var item in epsClosure)
                    {
                        if (!item.Contains('{'))
                        {
                            if (!currentStates.Contains(item))
                                currentStates.Add(item);
                            epsAdded = true;
                        }
                        else
                        {
                            var tempState = item.Trim('{', '}');
                            foreach (var state in tempState.Split(','))
                            {
                                if (!currentStates.Contains(state))
                                    currentStates.Add(state);
                            }
                            epsAdded = true;
                        }
                    }

                    if (epsAdded)
                    {
                        foreach (var item in prevCurrent)
                        {
                            Console.Write($"Состояние {item} образует следующее замыкание: ");
                            foreach (var state in allClosures[item])
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
                    foreach (var item in currentStates)
                    {
                        Console.Write($"'{item}', ");
                    }
                    Console.WriteLine("входит в число финальных состояний.");
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Ни одно из состояний ");
                    foreach (var item in currentStates)
                    {
                        Console.Write($"'{item}', ");
                    }
                    Console.WriteLine("не входит в число финальных состояний.");
                }
            }
            else if (!emergencyBreak && deadEnd)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Чтение строки невозможно продолжить");
            }
            return false;
        }

        public DnaAutomath ToNka()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nВыполняется преобразование НКА с ε-переходами к 'обычному' НКА:\n");

            List<string> newInputs = Inputs.ToList();
            newInputs.RemoveAt(Inputs.Length - 1);

            Dictionary<string, List<string>> closures = GetAllClosures();

            List<string> newStates = new List<string>(States);
            Dictionary<string, List<string>> newTransitions = new Dictionary<string, List<string>>();
            List<string> newFinalStates = new List<string>();

            var amountOfInputs = newInputs.Count;

            foreach (var state in States)
            {
                List<string> transitionsForState = new List<string>();

                for (var j = 0; j < amountOfInputs; j++)
                {
                    HashSet<string> targetStates = new HashSet<string>();

                    List<string> epsClosureOfState = closures.ContainsKey(state)
                                                        ? closures[state]
                                                        : new List<string>() { state };
                    if (!epsClosureOfState.Contains(state))
                        epsClosureOfState.Add(state);

                    foreach (var p in epsClosureOfState)
                    {
                        var trans = Transitions[p][j];
                        if (trans != "~")
                        {
                            var cleaned = trans.Trim();
                            List<string> nextStates;
                            if (cleaned.StartsWith("{") && cleaned.EndsWith("}"))
                            {
                                cleaned = cleaned.Trim('{', '}');
                                nextStates = cleaned.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(s => s.Trim()).ToList();
                            }
                            else
                            {
                                nextStates = new List<string>() { cleaned };
                            }
                            foreach (var r in nextStates)
                            {
                                if (closures.ContainsKey(r))
                                {
                                    foreach (var s in closures[r])
                                        targetStates.Add(s);
                                    targetStates.Add(r);
                                }
                                else
                                {
                                    targetStates.Add(r);
                                }
                            }
                        }
                    }
                    targetStates.Remove("~");
                    string transitionStr;
                    if (targetStates.Count == 0)
                        transitionStr = "~";
                    else if (targetStates.Count == 1)
                        transitionStr = targetStates.First();
                    else
                        transitionStr = "{" + string.Join(",", targetStates.OrderBy(x => x)) + "}";
                    transitionsForState.Add(transitionStr);
                }
                newTransitions[state] = transitionsForState;

                List<string> epsClosure = closures.ContainsKey(state) ? closures[state] : new List<string>();
                if (FinalStates.Intersect(epsClosure).Any() || FinalStates.Contains(state))
                {
                    if (!newFinalStates.Contains(state))
                        newFinalStates.Add(state);
                }
            }

            return new DnaAutomath(newStates.ToArray(), newInputs.ToArray(), newFinalStates.ToArray(), InitState, newTransitions);
        }


        public Dictionary<string, List<string>> GetAllClosures()
        {
            if (Type != TypeAutomaton.ENKA)
                return null;

            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            foreach (var state in Transitions.Keys)
            {
                HashSet<string> closure = new HashSet<string>();
                Queue<string> queue = new Queue<string>();

                closure.Add(state);
                queue.Enqueue(state);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    var epsilonTrans = Transitions[current][Inputs.Length - 1];
                    if (epsilonTrans != "~")
                    {
                        List<string> epsTargets;
                        var cleaned = epsilonTrans.Trim();
                        if (cleaned.StartsWith("{") && cleaned.EndsWith("}"))
                        {
                            cleaned = cleaned.Trim('{', '}');
                            epsTargets = cleaned.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(s => s.Trim()).ToList();
                        }
                        else
                        {
                            epsTargets = new List<string>() { cleaned };
                        }
                        foreach (var target in epsTargets)
                        {
                            if (!closure.Contains(target))
                            {
                                closure.Add(target);
                                queue.Enqueue(target);
                            }
                        }
                    }
                }
                result[state] = closure.ToList();
            }
            return result;
        }
    }
}
