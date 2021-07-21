using System;
using System.Linq;

namespace SudokuSolving
{
    public class Program
    {
        static void Main()
        {
            try
            {
                string sudo = GetSudoku();

                Sudoku sudoku = new(sudo);

                sudoku.Solve();

                var text = sudoku.IsSolved() ? "Sudoku was solved" : "Sudoku was not solved";

                string resultWithEnter = string.Join("\n", sudoku.ToString().Split('|').ToList().Split(9));

                Console.WriteLine($"{text}:\n\n{resultWithEnter}\n\n");

                if (!sudoku.IsSolved() && GetYesNoQuestion("Finish it by brute force?"))
                {
                    sudoku.SolveByBruteForce();

                    text = sudoku.IsSolved() ? "Sudoku was solved" : "Sudoku was not solved";

                    resultWithEnter = string.Join("\n", sudoku.ToString().Split('|').ToList().Split(9));

                    Console.WriteLine($"\n\n{text}:\n\n{resultWithEnter}\n\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n\nAn exception has occurred:\n\n{e.Message}\n\n");
            }

            Console.ReadLine();
        }

        private static string GetSudoku()
        {
            do
            {
                Console.WriteLine("Enter 81 numbers with 0 or Dot instead of empty:\n");

                string sudo = Console.ReadLine().Replace(".", "0");

                if (sudo.Length != 81) { Console.WriteLine($"INCORRECT LENGTH!! - {sudo.Length} instead of 81\n\n"); }
                else if (System.Text.RegularExpressions.Regex.Replace(sudo, "[^0-9]", "").Length != 81) { Console.WriteLine("INCORRECT VALUE!! - Enter only numbers or Dot\n\n"); }
                else { return sudo; }

            } while (true);
        }

        private static bool GetYesNoQuestion(string question)
        {
            do
            {
                Console.WriteLine($"{question} [S] / [N]");

                ConsoleKey ans = Console.ReadKey().Key;

                return ans switch
                {
                    ConsoleKey.S => true,
                    ConsoleKey.N => false,
                    _ => GetYesNoQuestion(question),
                };
            } while (true);
        }
    }
}