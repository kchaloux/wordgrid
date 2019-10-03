using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Math;

namespace WordGrid.Core
{
    /// <summary>
    /// A prefix tree node that represents a single character and each possible character that can occur after it.
    /// </summary>
    public class Prefix : IEnumerable<KeyValuePair<char, Prefix>>
    {
        /// <summary>
        /// Gets the character that this node represents.
        /// </summary>
        public char C { get; }

        /// <summary>
        /// Gets or Sets the child node at the given character.
        /// </summary>
        /// <param name="c">The character of the child node.</param>
        /// <returns>The child node with the given character.</returns>
        public Prefix this[char c]
        {
            get => _next[c];
            set => _next[c] = value;
        }

        /// <summary>
        /// Gets all children of this node.
        /// </summary>
        public IEnumerable<Prefix> Children => _next.Values;

        private readonly SortedList<char, Prefix> _next;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="c">The character that this node represents.</param>
        public Prefix(char c)
        {
            C = c;
            _next = new SortedList<char, Prefix>();
        }

        /// <summary>
        /// Empty Constructor.
        /// </summary>
        public Prefix() : this(' ')
        {
        }

        /// <summary>
        /// Check wether or not this node has any immediate children with the following character.
        /// </summary>
        /// <param name="c">The character to check for in this node's children.</param>
        /// <returns>True if one of this node's children has the given character.</returns>
        public bool Contains(char c)
        {
            return _next.ContainsKey(c);
        }

        /// <summary>
        /// Add a child node.
        /// </summary>
        /// <param name="value">The child node to add.</param>
        public void Add(Prefix value)
        {
            _next.Add(value.C, value);
        }

        /// <summary>
        /// Attempt to get the child node with the given character.
        /// </summary>
        /// <param name="c">The character of the child node to get.</param>
        /// <param name="value">The child node with the given character, if any.</param>
        /// <returns>True if a child exists with the given character, otherwise false.</returns>
        public bool TryGetValue(char c, out Prefix value)
        {
            return _next.TryGetValue(c, out value);
        }

        /// <summary>
        /// Iterate through each string that can be spelled by traversing all children of this node.
        /// </summary>
        /// <returns>Each string that can be spelled by traversing all children of this node.</returns>
        public IEnumerable<string> TraverseWords()
        {
            var builder = new StringBuilder();
            var stack = new Stack<Queue<Prefix>>(new[] { new Queue<Prefix>(Children) });
            var nodes = new Stack<Prefix>(new[] { this });
            while (stack.Count > 0)
            {
                var node = nodes.Peek();
                var children = stack.Peek();
                if (children.Any())
                {
                    var child = children.Dequeue();
                    builder.Append(child);
                    if (node.TryGetValue(child.C, out var nextNode) && nextNode.Any())
                    {
                        stack.Push(new Queue<Prefix>(nextNode.Children));
                        nodes.Push(nextNode);
                    }
                    else
                    {
                        yield return builder.ToString();
                        builder.Length--;
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
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{C}";
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<char, Prefix>> GetEnumerator()
        {
            return _next.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
