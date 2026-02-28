using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

[DebuggerDisplay("Count = {CountWithCheck,nq}")]
[DebuggerTypeProxy(typeof(SyntaxList<>.DebuggerProxy))]
public readonly struct SyntaxList<TNode> : IReadOnlyList<TNode> where TNode : SyntaxNode
{
	private readonly TNode[] _nodes;

	public int Count => _nodes.Length;

	public bool IsEmpty => _nodes.Length == 0;

	public bool IsDefault => _nodes is null;

	public bool IsDefaultOrEmpty => _nodes is null || _nodes.Length == 0;

	public int Position
	{
		get
		{
			if (IsDefaultOrEmpty)
			{
				return default;
			}

			return _nodes[0].Span.Start;
		}
	}

	public Location Location
	{
		get
		{
			if(IsDefaultOrEmpty)
			{
				return Location.Invalid;
			}

			Location location = _nodes[0].Location;
			TextSpan end = _nodes[^1].Span;

			return location.MoveEnd(end.End);
		}
	}

	public TextSpan Span
	{
		get
		{
			if(IsDefaultOrEmpty)
			{
				return default;
			}

			return _nodes[0].Span.MoveEnd(_nodes[^1].Span.End);
		}
	}

	private int CountWithCheck => _nodes is null ? 0 : _nodes.Length;

	public TNode this[int index] => _nodes[index];

	internal SyntaxList(TNode[] nodes)
	{
		_nodes = nodes;
	}

	public override string ToString()
	{
		return ToString(' ');
	}

	public string ToString(char separator)
	{
		if (_nodes is null || _nodes.Length == 0)
		{
			return "";
		}

		StringBuilder sb = new();

		sb.Append(_nodes[0].ToString());

		for (int i = 1; i < _nodes.Length; i++)
		{
			sb.Append(separator);
			sb.Append(_nodes[i].ToString());
		}

		return sb.ToString();
	}

	public bool Any()
	{
		return !IsDefaultOrEmpty;
	}

	public bool HasKind(SyntaxKind kind)
	{
		if(IsDefault)
		{
			return false;
		}

		for (int i = 0; i < _nodes.Length; i++)
		{
			if (_nodes[0].Kind == kind)
			{
				return true;
			}
		}

		return false;
	}

	public Enumerator GetEnumerator()
	{
		if (IsDefault)
		{
			return new(Array.Empty<TNode>());
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
		private readonly TNode[] _nodes;
		private int _index;

		public readonly TNode Current => _nodes[_index];

		readonly object IEnumerator.Current => Current;

		internal Enumerator(TNode[] nodes)
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
		private readonly SyntaxList<TNode> _list;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public TNode[] Items
		{
			get
			{
				TNode[] items = new TNode[_list.Count];
				_list._nodes.CopyTo(items, 0);
				return items;
			}
		}

		public DebuggerProxy(SyntaxList<TNode> list)
		{
			_list = list;
		}
	}
}
