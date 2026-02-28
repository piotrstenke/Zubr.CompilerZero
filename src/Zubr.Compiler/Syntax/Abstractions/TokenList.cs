using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Syntax.Abstractions;

[DebuggerDisplay("Count = {CountWithCheck,nq}")]
[DebuggerTypeProxy(typeof(DebuggerProxy))]
public readonly struct TokenList : IReadOnlyCollection<Token>
{
	private readonly Token[] _tokens;

	public static TokenList Default => default;

	public static TokenList Empty => new(Array.Empty<Token>());

	public int Count => _tokens.Length;

	public bool IsEmpty => _tokens.Length == 0;

	public bool IsDefault => _tokens is null;

	public bool IsDefaultOrEmpty => _tokens is null || _tokens.Length == 0;

	private int CountWithCheck => _tokens is null ? 0 : _tokens.Length;

	public int Position
	{
		get
		{
			if (IsDefaultOrEmpty)
			{
				return default;
			}

			return _tokens[0].Position;
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

			ref readonly Token first = ref _tokens[0];
			ref readonly Token last = ref _tokens[^1];

			return first.Span.MoveEnd(last.Span.End);
		}
	}

	public Token this[int index] => _tokens[index];

	internal TokenList(Token[] tokens)
	{
		_tokens = tokens;
	}

	public override string ToString()
	{
		return ToString(' ');
	}

	public string ToString(char separator)
	{
		if (_tokens is null || _tokens.Length == 0)
		{
			return "";
		}

		StringBuilder sb = new();

		sb.Append(_tokens[0].ToString());

		for (int i = 1; i < _tokens.Length; i++)
		{
			sb.Append(separator);
			sb.Append(_tokens[i].ToString());
		}

		return sb.ToString();
	}

	public bool Any()
	{
		return !IsDefaultOrEmpty;
	}

	public bool HasKind(TokenKind kind)
	{
		if (IsDefault)
		{
			return false;
		}

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
			return new(Array.Empty<Token>());
		}

		return new(_tokens);
	}

	IEnumerator<Token> IEnumerable<Token>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public struct Enumerator : IEnumerator<Token>
	{
		private readonly Token[] _tokens;
		private int _index;

		public readonly Token Current => _tokens[_index];

		readonly object IEnumerator.Current => Current;

		internal Enumerator(Token[] tokens)
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
		private readonly TokenList _list;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public Token[] Items
		{
			get
			{
				Token[] items = new Token[_list.Count];
				_list._tokens.CopyTo(items, 0);
				return items;
			}
		}

		public DebuggerProxy(TokenList list)
		{
			_list = list;
		}
	}
}
