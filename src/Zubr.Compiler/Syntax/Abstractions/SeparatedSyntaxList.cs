using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

[DebuggerDisplay("Count = {CountWithCheck,nq}")]
[DebuggerTypeProxy(typeof(SeparatedSyntaxList<>.DebuggerProxy))]
public readonly struct SeparatedSyntaxList<TNode> : IReadOnlyList<TNode> where TNode : SyntaxNode
{
	private readonly (TNode node, Token separator)[] _nodes;

	public int Count => _nodes.Length;

	public bool IsEmpty => _nodes.Length == 0;

	public bool IsDefault => _nodes is null;

	public bool IsDefaultOrEmpty => _nodes is null || _nodes.Length == 0;

	private int CountWithCheck => _nodes is null ? 0 : _nodes.Length;

	public int Position
	{
		get
		{
			if (IsDefaultOrEmpty)
			{
				return default;
			}

			return _nodes[0].node.Span.Start;
		}
	}

	public Location Location
	{
		get
		{
			if (IsDefaultOrEmpty)
			{
				return Location.Invalid;
			}

			Location location = _nodes[0].node.Location;
			TextSpan end = _nodes[^1].node.Span;

			return location.MoveEnd(end.End);
		}
	}

	public TextSpan Span
	{
		get
		{
			if (IsDefaultOrEmpty)
			{
				return default;
			}

			return _nodes[0].node.Span.MoveEnd(_nodes[^1].node.Span.End);
		}
	}

	public TNode this[int index] => _nodes[index].node;

	internal SeparatedSyntaxList((TNode node, Token separator)[] nodes)
	{
		_nodes = nodes;
	}

	public override string ToString()
	{
		if (_nodes is null || _nodes.Length == 0)
		{
			return "";
		}

		StringBuilder sb = new();

		sb.Append(_nodes[0].node.ToString());

		for (int i = 1; i < _nodes.Length; i++)
		{
			sb.Append(_nodes[i - 1].separator.ToString());
			sb.Append(' ');
			sb.Append(_nodes[i].node.ToString());
		}

		return sb.ToString();
	}

	public bool Any()
	{
		return !IsDefaultOrEmpty;
	}

	public bool HasKind(SyntaxKind kind)
	{
		if (IsDefault)
		{
			return false;
		}

		for (int i = 0; i < _nodes.Length; i++)
		{
			if (_nodes[0].node.Kind == kind)
			{
				return true;
			}
		}

		return false;
	}

	public Token GetSeparator(int index)
	{
		return _nodes[index].separator;
	}

	public TokenList GetSeparators()
	{
		Token[] separators = new Token[_nodes.Length - 1];

		for (int i = 0; i < separators.Length; i++)
		{
			separators[i] = _nodes[i].separator;
		}

		return new TokenList(separators);
	}

	public Enumerator GetEnumerator()
	{
		if (IsDefault)
		{
			return new(Array.Empty<(TNode, Token)>());
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
		private readonly (TNode node, Token)[] _nodes;
		private int _index;

		public readonly TNode Current => _nodes[_index].node;

		readonly object IEnumerator.Current => Current;

		internal Enumerator((TNode node, Token separator)[] nodes)
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
