using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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

	private int CountWithCheck => _nodes is null ? 0 : _nodes.Length;

	public TNode this[int index] => _nodes[index];

	internal SyntaxList(TNode[] nodes)
	{
		_nodes = nodes;
	}

	public Enumerator GetEnumerator()
	{
		if(IsDefault)
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
