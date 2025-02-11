using Lab0.Classes;
using System;

namespace Lab0
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"Resources\DefaultAutomath.txt";
            Automath automaton = Automath.CreateFromFile(path);
            if (automaton == null)
                return;

            automaton.ShowInfo();
            Console.WriteLine();
            automaton.ShowTable();

            Console.Write("\nВведите входное слово: ");
            string inLine = Console.ReadLine();
            automaton.ProcessInputLine(inLine);

            if (automaton.Type == TypeAutomaton.NKA)
            {
                DnaAutomath nka = automaton as DnaAutomath;
                DkaAutomath dka = nka.ToDka();
                if (dka != null)
                {
                    dka.ShowInfo();
                    dka.ShowTable();
                }
            }

            if (automaton.Type == TypeAutomaton.ENKA)
            {
                DnaEpsilonAutomath enka = automaton as DnaEpsilonAutomath;
                DnaAutomath nka = enka.ToNka();
                nka.ShowInfo();
                nka.ShowTable();
            }
        }
    }
}
