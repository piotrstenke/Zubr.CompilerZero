using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Zubr.Compiler.Syntax.Abstractions;

[DebuggerDisplay("Count = {Count,nq}")]
[DebuggerTypeProxy(typeof(SeparatedSyntaxList<>.DebuggerProxy))]
public readonly struct SeparatedSyntaxList<TNode> : IReadOnlyList<TNode> where TNode : SyntaxNode
{
	private readonly (TNode node, SyntaxToken separator)[] _nodes;

	public int Count => _nodes.Length;

	public bool IsEmpty => _nodes.Length == 0;

	public bool IsDefault => _nodes is null;

	public TNode this[int index] => _nodes[index].node;

	internal SeparatedSyntaxList((TNode node, SyntaxToken separator)[] nodes)
	{
		_nodes = nodes;
	}

	public SyntaxToken GetSeparator(int index)
	{
		return _nodes[index].separator;
	}

	public SyntaxTokenList GetSeparators()
	{
		SyntaxToken[] separators = new SyntaxToken[_nodes.Length - 1];

		for (int i = 0; i < separators.Length; i++)
		{
			separators[i] = _nodes[i].separator;
		}

		return new SyntaxTokenList(separators);
	}

	public Enumerator GetEnumerator()
	{
		if (IsDefault)
		{
			return new(Array.Empty<(TNode, SyntaxToken)>());
		}

		return new(_nodes);
	}

	IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public struct Enumerator : IEnumerator<TNode>
	{
		private readonly (TNode node, SyntaxToken)[] _nodes;
		private int _index;

		public readonly TNode Current => _nodes[_index].node;

		readonly object IEnumerator.Current => Current;

		internal Enumerator((TNode node, SyntaxToken separator)[] nodes)
		{
			_nodes = nodes;
			_index = -1;
		}

		public bool MoveNext()
		{
			if (_index < _nodes.Length - 1)
			{
				_index++;

				return true;
			}

			return false;
		}

		void IEnumerator.Reset()
		{
			_index = -1;
		}

		readonly void IDisposable.Dispose()
		{
		}
	}

	private sealed class DebuggerProxy
	{
		private readonly SeparatedSyntaxList<TNode> _list;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public TNode[] Items
		{
			get
			{
				TNode[] items = new TNode[_list.Count];

				for (int i = 0; i < items.Length; i++)
				{
					items[i] = _list[i];
				}

				return items;
			}
		}

		public DebuggerProxy(SeparatedSyntaxList<TNode> list)
		{
			_list = list;
		}
	}
}
