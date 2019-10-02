using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordGrid
{
    /// <summary>
    /// Contains the text of a word grid board.
    /// </summary>
    public class Board
    {
        /// <summary>
        /// Gets the size of the board.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets or Sets the character at the specified row and column in the board.
        /// </summary>
        /// <param name="i">The row of the character to get.</param>
        /// <param name="j">The column of the character to get.</param>
        /// <returns></returns>
        public char this[int i, int j]
        {
            get => _chars[i, j];
            set => SetCell(i, j, value);
        }

        /// <summary>
        /// Gets the board's rows.
        /// </summary>
        public IEnumerable<string> Rows => Enumerable.Range(0, Size)
            .Select(i => string.Concat(Enumerable.Range(0, Size).Select(j => _chars[i, j])));

        /// <summary>
        /// Gets the board's columns.
        /// </summary>
        public IEnumerable<string> Columns => Enumerable.Range(0, Size)
            .Select(i => string.Concat(Enumerable.Range(0, Size).Select(j => _chars[j, i])));

        private readonly char[,] _chars;
        private readonly Dictionary<int, char>[] _rowConstraints;
        private readonly Dictionary<int, char>[] _columnConstraints;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="size">The size of the board.</param>
        public Board(int size)
        {
            Size = size;
            _chars = new char[Size, Size];
            _rowConstraints = Enumerable.Range(0, Size).Select(_ => new Dictionary<int, char>()).ToArray();
            _columnConstraints = Enumerable.Range(0, Size).Select(_ => new Dictionary<int, char>()).ToArray();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rows">The rows to fill the board with.</param>
        public Board(IReadOnlyList<string> rows)
        {
            Size = rows.Count;
            _chars = new char[Size, Size];
            _rowConstraints = new Dictionary<int, char>[Size];
            _columnConstraints = new Dictionary<int, char>[Size];
            for (var i = 0; i < Size; ++i)
            {
                if (rows[i].Length != Size)
                {
                    throw new ArgumentOutOfRangeException(nameof(rows), "All rows and columns must be the same size");
                }
                for (var j = 0; j < Size; ++j)
                {
                    _chars[i, j] = rows[i][j];
                }
                _rowConstraints[i] = new Dictionary<int, char>();
                _columnConstraints[i] = new Dictionary<int, char>();
            }
        }

        /// <summary>
        /// Copy Constructor.
        /// </summary>
        /// <param name="other">The board to copy.</param>
        public Board(Board other)
        {
            Size = other.Size;
            Array.Copy(other._chars, _chars, _chars.Length);
            _rowConstraints = other._rowConstraints.Select(x => new Dictionary<int, char>(x)).ToArray();
            _columnConstraints = other._columnConstraints.Select(x => new Dictionary<int, char>(x)).ToArray();
        }

        /// <summary>
        /// Create a deep copy of this board.
        /// </summary>
        /// <returns>A new board with the same contents as this one.</returns>
        public Board Clone()
        {
            return new Board(this);
        }

        /// <summary>
        /// Set the character in a specific cell of the board.
        /// </summary>
        /// <param name="row">The index of the row to set.</param>
        /// <param name="column">The index of the column to set.</param>
        /// <param name="c">The character to set in the cell at the given indices.</param>
        public void SetCell(int row, int column, char c)
        {
            c = char.ToLowerInvariant(c);
            if (row < 0 || row >= Size) throw new IndexOutOfRangeException(nameof(row));
            if (column < 0 || column >= Size) throw new IndexOutOfRangeException(nameof(column));
            if (c != ' ' && (c < 'a' || c > 'z')) throw new ArgumentOutOfRangeException(nameof(c));

            _chars[row, column] = c;
            if (c == ' ')
            {
                _rowConstraints[row].Remove(column);
                _columnConstraints[column].Remove(row);
            }
            else
            {
                _rowConstraints[row][column] = c;
                _columnConstraints[column][row] = c;
            }
        }

        /// <summary>
        /// Set the text of the row at the given index.
        /// </summary>
        /// <param name="index">The index of the row to set.</param>
        /// <param name="word">The word to put in the row.</param>
        public void SetRow(int index, string word)
        {
            for (var i = 0; i < word.Length; ++i)
            {
                SetCell(index, i, word[i]);
            }
        }

        /// <summary>
        /// Set the text of the column at the given index.
        /// </summary>
        /// <param name="index">The index of the column to set.</param>
        /// <param name="word">The word to put in the column.</param>
        public void SetColumn(int index, string word)
        {
            for (var i = 0; i < word.Length; ++i)
            {
                SetCell(i, index, word[i]);
            }
        }

        /// <summary>
        /// Gets a mapping of indices to non-empty characters for the row at the given index.
        /// </summary>
        /// <param name="index">The index of the row to get the constraints for.</param>
        /// <returns>A dictionary mapping the each non-empty character from its index at the given row.</returns>
        public IReadOnlyDictionary<int, char> GetRowConstraints(int index)
        {
            return _rowConstraints[index];
        }

        /// <summary>
        /// Gets a mapping of indices to non-empty characters for the column at the given index.
        /// </summary>
        /// <param name="index">The index of the column to get the constraints for.</param>
        /// <returns>A dictionary mapping the each non-empty character from its index at the given column.</returns>
        public IReadOnlyDictionary<int, char> GetColumnConstraints(int index)
        {
            return _columnConstraints[index];
        }

        /// <summary>
        /// Check if the given word is contained in the board's rows or columns.
        /// </summary>
        /// <param name="word">The word to look for.</param>
        /// <returns>True if the given word is contained in at least one of the board's rows or columns.</returns>
        public bool Contains(string word)
        {
            if (word.Length != Size)
            {
                return false;
            }

            for (var i = 0; i < Size; ++i)
            {
                // check each row for the given word
                if (_chars[i, 0] == word[0])
                {
                    for (var j = 1; j < Size; ++j)
                    {
                        if (_chars[i, j] != word[j]) break;
                        if (j == Size - 1) return true;
                    }
                }
                // check each column for the given word
                if (_chars[0, i] == word[0])
                {
                    for (var j = 1; j < Size; ++j)
                    {
                        if (_chars[j, i] != word[j]) break;
                        if (j == Size - 1) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Clear the board.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < Size; ++i)
            {
                for (var j = 0; j < Size; ++j)
                {
                    _chars[i, j] = ' ';
                }
                _rowConstraints[i].Clear();
                _columnConstraints[i].Clear();
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < Size; ++i)
            {
                for (var j = 0; j < Size; ++j)
                {
                    builder.Append(_chars[i, j]);
                }
                if (i < Size - 1)
                {
                    builder.AppendLine();
                }
            }
            return builder.ToString();
        }
    }
}
