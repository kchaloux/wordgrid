using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace WordGrid
{
    /// <summary>
    /// Finds solutions for boards from a given set of words.
    /// </summary>
    public class Solver : IEnumerable<string>
    {
        /// <summary>
        /// Gets the size of the words that this solver uses.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Gets the total number of words loaded into this solver's word list.
        /// </summary>
        public int Count => _words.Count;

        private Prefix _root;
        private List<string> _words;

        /// <summary>
        /// Load all words of the given size from the specified file into the solver.
        /// </summary>
        /// <param name="file">The file to load the words from.</param>
        /// <param name="size">The length of the words to load into the solver.</param>
        public void Initialize(string file, int size)
        {
            _root = new Prefix();
            Size = size;
            _words = new List<string>();

            using (var reader = File.OpenText(file))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var stripped = line.Trim().ToLowerInvariant();
                    if (stripped.Length == size && stripped.All(c => c >= 'a' && c <= 'z'))
                    {
                        _words.Add(stripped);
                        var node = _root;
                        foreach (var c in stripped)
                        {
                            if (!node.Contains(c))
                            {
                                node.Add(new Prefix(c));
                            }
                            node = node[c];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check whether or not the given word is loaded into the solver.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public bool Contains(string word)
        {
            if (word.Length != Size)
            {
                return false;
            }
            return FindNode(word) != null;
        }

        /// <summary>
        /// Iterate through the possible solutions for the given board.
        /// </summary>
        /// <param name="board">The board to solve.</param>
        /// <returns>Each solution for the given board.</returns>
        public IEnumerable<Board> FindSolutions(Board board)
        {
            var initialSolutions = FindRowSolutions(board, new string[0], 0);
            return ProcessSolutions(board, initialSolutions);
        }

        /// <summary>
        /// Find every solution for the given board.
        /// </summary>
        /// <param name="board">The board to solve.</param>
        /// <returns>A list of solutions to the given board.</returns>
        /// <remarks>
        /// This search is parallelized across all available cores, allowing it to solve the board much faster
        /// at the cost of being unable to yield results as they are discovered.
        /// </remarks>
        public IReadOnlyList<Board> FindAllSolutions(Board board)
        {
            var availableCores = Environment.ProcessorCount;

            var initialSolutions = new Queue<string>[availableCores];
            var results = new List<Board>[availableCores];
            for (var i = 0; i < availableCores; ++i)
            {
                results[i] = new List<Board>();
                initialSolutions[i] = new Queue<string>();
            }

            var core = 0;
            foreach (var solution in FindRowSolutions(board, new string[0], 0))
            {
                initialSolutions[core].Enqueue(solution);
                core = (core + 1) % availableCores;
            }

            Parallel.For(0, availableCores, i =>
            {
                results[i] = ProcessSolutions(board, initialSolutions[i]).ToList();
            });
            return results.SelectMany(x => x).ToList();
        }

        /// <summary>
        /// Solve the given board for each word in the given initial solutions as the first row.
        /// </summary>
        /// <param name="board">The board to solve.</param>
        /// <param name="initialSolutions">Each solution for the first row to solve for.</param>
        /// <returns>Each solution for the given board and first row solutions.</returns>
        private IEnumerable<Board> ProcessSolutions(Board board, IEnumerable<string> initialSolutions)
        {
            var rows = new List<string>();
            var solutions = new Stack<Queue<string>>(new[] { new Queue<string>(initialSolutions) });
            while (solutions.Count > 0)
            {
                if (solutions.Peek().Count > 0)
                {
                    rows.Add(solutions.Peek().Dequeue());
                    if (rows.Count >= Size)
                    {
                        var solvedBoard = new Board(rows);
                        if (!solvedBoard.Rows.Intersect(solvedBoard.Columns).Any())
                        {
                            yield return solvedBoard;
                        }
                        rows.RemoveAt(rows.Count - 1);
                    }
                    else
                    {
                        solutions.Push(new Queue<string>(FindRowSolutions(board, rows, rows.Count)));
                    }
                }
                else
                {
                    solutions.Pop();
                    if (rows.Any())
                    {
                        rows.RemoveAt(rows.Count - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Find every word that could potentially solve the current board at the given row.
        /// </summary>
        /// <param name="board">The board to solve.</param>
        /// <param name="rows">The current rows of the solution.</param>
        /// <param name="rowIndex">The index of the row to solve for.</param>
        /// <returns>Each word that could potentially solve the board at the given row.</returns>
        private IEnumerable<string> FindRowSolutions(Board board, IReadOnlyList<string> rows, int rowIndex)
        {
            var nextLetters = new List<IReadOnlyList<char>>();
            for (var i = 0; i < Size; ++i)
            {
                var columnIndex = i;
                var prefix = string.Concat(rows.Select(row => row[columnIndex]).Take(rowIndex));
                if (board.GetRowConstraints(rowIndex).TryGetValue(i, out var c))
                {
                    nextLetters.Add(new[] { c });
                }
                else
                {
                    var columnLetters = FindNextLetters(prefix, board.GetColumnConstraints(columnIndex));
                    if (!columnLetters.Any())
                    {
                        yield break;
                    }
                    nextLetters.Add(columnLetters);
                }
            }

            var builder = new StringBuilder();
            var stack = new Stack<Queue<char>>(new[] { new Queue<char>(nextLetters[0]) });
            var nodes = new Stack<Prefix>(new[] { _root });
            while (stack.Count > 0)
            {
                var cs = stack.Peek();
                var node = nodes.Peek();
                if (cs.Any())
                {
                    var c = cs.Dequeue();
                    if (node.TryGetValue(c, out var nextNode))
                    {
                        builder.Append(c);
                        var text = builder.ToString();
                        if (text.Length == Size)
                        {
                            if (!rows.Contains(text))
                            {
                                yield return text;
                            }
                            builder.Length--;
                        }
                        else
                        {
                            stack.Push(new Queue<char>(nextLetters[builder.Length].Intersect(FindNextLetters(text))));
                            nodes.Push(nextNode);
                        }
                    }
                }
                else
                {
                    stack.Pop();
                    nodes.Pop();
                    builder.Length = Max(0, builder.Length - 1);
                }
            }
        }

        /// <summary>
        /// Find the next letters that could potentially follow the given prefix and still create a valid word.
        /// </summary>
        /// <param name="prefix">The prefix of the word to find the next letters to.</param>
        /// <param name="constraints">Optional constraints specifying letters later in the word.</param>
        /// <returns>A list of letters that could continue the given prefix and still create a valid word.</returns>
        private IReadOnlyList<char> FindNextLetters(
            string prefix,
            IReadOnlyDictionary<int, char> constraints = null)
        {
            var root = FindNode(prefix);
            if (root == null)
            {
                return new char[0];
            }

            var maxDepth = constraints?.Keys.DefaultIfEmpty(-1).Max() ?? -1;
            if (constraints == null || maxDepth < prefix.Length)
            {
                return root.Children.Select(node => node.C).ToList();
            }

            var letters = new List<char>();
            foreach (var c in root.Children.Select(child => child.C))
            {
                var stack = new Stack<Queue<char>>(new[] { new Queue<char>(new[] { c }) });
                var nodes = new Stack<Prefix>(new[] { root });
                while (stack.Count > 0)
                {
                    var cs = stack.Peek();
                    var node = nodes.Peek();
                    var depth = prefix.Length + stack.Count - 1;
                    Prefix nextNode = null;
                    if (constraints.TryGetValue(depth, out var constraint) && cs.Contains(constraint))
                    {
                        if (depth >= maxDepth)
                        {
                            letters.Add(c);
                            break;
                        }
                        node.TryGetValue(constraint, out nextNode);
                    }
                    else if (cs.Any())
                    {
                        node.TryGetValue(cs.Peek(), out nextNode);
                    }

                    if (nextNode != null && nextNode.Any())
                    {
                        stack.Push(new Queue<char>(nextNode.Children.Select(child => child.C)));
                        nodes.Push(nextNode);
                    }
                    else
                    {
                        stack.Pop();
                        if (stack.Count > 0 && stack.Peek().Count > 0)
                        {
                            stack.Peek().Dequeue();
                        }
                        nodes.Pop();
                    }
                }
            }
            return letters;
        }

        /// <summary>
        /// Find the node from the root that matches the given prefix, if any.
        /// </summary>
        /// <param name="prefix">The prefix to search for.</param>
        /// <returns>The prefix node for the given text, or null if none exists.</returns>
        private Prefix FindNode(string prefix)
        {
            var node = _root;
            foreach (var c in prefix)
            {
                if (!node.TryGetValue(c, out node))
                {
                    return null;
                }
            }
            return node;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<string> GetEnumerator()
        {
            return _words.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
