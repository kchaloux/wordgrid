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
        /// Gets the character at the specified row and column in the board.
        /// </summary>
        /// <param name="i">The row of the character to get.</param>
        /// <param name="j">The column of the character to get.</param>
        /// <returns></returns>
        public char this[int i, int j] => _rows[i][j];

        /// <summary>
        /// Gets the board's rows.
        /// </summary>
        public IReadOnlyList<string> Rows => _rows;

        /// <summary>
        /// Gets the board's columns.
        /// </summary>
        public IReadOnlyList<string> Columns
        {
            get
            {
                UpdateProperties();
                return _columns;
            }
        }

        private readonly string[] _rows;
        private readonly string[] _columns;
        private readonly List<SortedList<int, char>> _rowConstraints;
        private readonly List<SortedList<int, char>> _columnConstraints;
        private bool _isUpToDate;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="size">The size of the board.</param>
        public Board(int size)
        {
            Size = size;
            _rows = new string[size];
            _columns = new string[size];
            _rowConstraints = new List<SortedList<int, char>>();
            _columnConstraints = new List<SortedList<int, char>>();

            var blankRow = string.Concat(Enumerable.Range(0, size).Select(_ => ' '));
            for (var i = 0; i < size; ++i)
            {
                _rows[i] = blankRow;
                _columns[i] = blankRow;
                _rowConstraints.Add(new SortedList<int, char>());
                _columnConstraints.Add(new SortedList<int, char>());
            }
            _isUpToDate = true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rows">The rows to fill the board with.</param>
        public Board(IReadOnlyList<string> rows)
        {
            Size = rows.Count;
            _rows = new string[Size];
            _columns = new string[Size];
            _rowConstraints = new List<SortedList<int, char>>();
            _columnConstraints = new List<SortedList<int, char>>();

            for (var i = 0; i < Size; ++i)
            {
                if (rows[i].Length != Size)
                {
                    throw new ArgumentOutOfRangeException(nameof(rows), "All rows and columns must be the same size");
                }
                _rows[i] = rows[i];
                _rowConstraints.Add(new SortedList<int, char>());
                _columnConstraints.Add(new SortedList<int, char>());
            }
            UpdateProperties();
        }

        /// <summary>
        /// Copy Constructor.
        /// </summary>
        /// <param name="other">The board to copy.</param>
        public Board(Board other)
        {
            Size = other.Size;
            _rows = other._rows.ToArray();
            _columns = other._columns.ToArray();
            _rowConstraints = other._rowConstraints.Select(x => new SortedList<int, char>(x)).ToList();
            _columnConstraints = other._columnConstraints.Select(x => new SortedList<int, char>(x)).ToList();
            _isUpToDate = other._isUpToDate;
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
        /// Set the text of the row at the given index.
        /// </summary>
        /// <param name="index">The index of the row to set.</param>
        /// <param name="word">The word to put in the row.</param>
        public void SetRow(int index, string word)
        {
            if (word.Length != Size)
            {
                throw new ArgumentOutOfRangeException(nameof(word), $"Word must be {Size} characters long");
            }
            _rows[index] = word;
            _isUpToDate = false;
        }

        /// <summary>
        /// Set the text of the column at the given index.
        /// </summary>
        /// <param name="index">The index of the column to set.</param>
        /// <param name="word">The word to put in the column.</param>
        public void SetColumn(int index, string word)
        {
            if (word.Length != Size)
            {
                throw new ArgumentOutOfRangeException(nameof(word), $"Word must be {Size} characters long");
            }
            for (var i = 0; i < Size; ++i)
            {
                _rows[i] = _rows[i].Substring(0, index) + word[i] + _rows[i].Substring(index + 1, _rows.Length - index - 1);
            }
            _isUpToDate = false;
        }

        /// <summary>
        /// Gets a mapping of indices to non-empty characters for the row at the given index.
        /// </summary>
        /// <param name="index">The index of the row to get the constraints for.</param>
        /// <returns>A dictionary mapping the each non-empty character from its index at the given row.</returns>
        public IReadOnlyDictionary<int, char> GetRowConstraints(int index)
        {
            UpdateProperties();
            return _rowConstraints[index];
        }

        /// <summary>
        /// Gets a mapping of indices to non-empty characters for the column at the given index.
        /// </summary>
        /// <param name="index">The index of the column to get the constraints for.</param>
        /// <returns>A dictionary mapping the each non-empty character from its index at the given column.</returns>
        public IReadOnlyDictionary<int, char> GetColumnConstraints(int index)
        {
            UpdateProperties();
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
            UpdateProperties();
            return _rows.Contains(word) || _columns.Contains(word);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Join(Environment.NewLine, _rows);
        }

        private void UpdateProperties()
        {
            if (_isUpToDate)
            {
                return;
            }
            _rowConstraints.ForEach(c => c.Clear());
            _columnConstraints.ForEach(c => c.Clear());
            for (var i = 0; i < Size; ++i)
            {
                var builder = new StringBuilder();
                for (var j = 0; j < Size; ++j)
                {
                    builder.Append(_rows[j][i]);
                    if (_rows[i][j] != ' ')
                    {
                        _rowConstraints[i][j] = _rows[i][j];
                    }
                    if (_rows[j][i] != ' ')
                    {
                        _columnConstraints[i][j] = _rows[j][i];
                    }
                }
                _columns[i] = builder.ToString();
            }
            _isUpToDate = true;
        }
    }
}
