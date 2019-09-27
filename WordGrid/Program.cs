using System;
using System.Linq;

namespace WordGrid
{
    class Program
    {
        static void Main(string[] args)
        {
            var solver = new Solver();
            solver.Initialize(@"Resources/words.txt", 5);
            var board = new Board(solver.Size);
            board.SetRow(2, "tepid");

            foreach (var solution in solver.FindSolutions(board).Take(5))
            {
                Console.WriteLine(solution);
                Console.WriteLine();
            }

            Console.ReadLine();
        }
    }
}
