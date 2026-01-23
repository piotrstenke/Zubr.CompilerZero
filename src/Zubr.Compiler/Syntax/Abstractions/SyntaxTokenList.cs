using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Zubr.Compiler.Syntax.Abstractions;

[DebuggerDisplay("Count = {CountWithCheck,nq}")]
[DebuggerTypeProxy(typeof(DebuggerProxy))]
public readonly struct SyntaxTokenList : IReadOnlyCollection<SyntaxToken>
{
	private readonly SyntaxToken[] _tokens;

	public int Count => _tokens.Length;

	public bool IsEmpty => _tokens.Length == 0;

	public bool IsDefault => _tokens is null;

	public bool IsDefaultOrEmpty => _tokens is null || _tokens.Length == 0;

	private int CountWithCheck => _tokens is null ? 0 : _tokens.Length;

	public SyntaxToken this[int index] => _tokens[index];

	public static SyntaxTokenList Empty => new(Array.Empty<SyntaxToken>());

	internal SyntaxTokenList(SyntaxToken[] tokens)
	{
		_tokens = tokens;
	}

	public bool HasKind(SyntaxKind kind)
	{
		for (int i = 0; i < _tokens.Length; i++)
		{
			if (_tokens[i].Kind == kind)
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
			return new(Array.Empty<SyntaxToken>());
		}

		return new(_tokens);
	}

	IEnumerator<SyntaxToken> IEnumerable<SyntaxToken>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public struct Enumerator : IEnumerator<SyntaxToken>
	{
		private readonly SyntaxToken[] _tokens;
		private int _index;

		public readonly SyntaxToken Current => _tokens[_index];

		readonly object IEnumerator.Current => Current;

		internal Enumerator(SyntaxToken[] tokens)
		{
			_tokens = tokens;
			_index = -1;
		}

		public bool MoveNext()
		{
			if (_index < _tokens.Length - 1)
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
		private readonly SyntaxTokenList _list;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public SyntaxToken[] Items
		{
			get
			{
				SyntaxToken[] items = new SyntaxToken[_list.Count];
				_list._tokens.CopyTo(items, 0);
				return items;
			}
		}

		public DebuggerProxy(SyntaxTokenList list)
		{
			_list = list;
		}
	}
}
