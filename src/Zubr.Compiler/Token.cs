using System.Diagnostics;
using Zubr.Compiler.Text;

namespace Zubr.Compiler;

[DebuggerDisplay("{Text ?? string.Empty,nq}")]
public readonly struct Token
{
	public object? Value { get; }

	public string Text { get; }

	public int Position { get; }

	public TokenKind Kind { get; }

	public TokenKind ContextualKind => SyntaxFacts.GetContextualKind(this);

	public int Length => Text.Length;

	public bool IsNone => Kind == TokenKind.None;

	public bool IsAny => !IsNone;

	public bool IsEmpty => Text.Length == 0;

	public TextSpan Span => new(Position, Length);

	internal Token(TokenKind kind, string text, int position)
	{
		Kind = kind;
		Text = text;
		Position = position;
	}

	internal Token(TokenKind kind, string text, int position, object? value)
	{
		Value = value;
		Kind = kind;
		Text = text;
		Position = position;
	}

	public override string ToString()
	{
		return Text ?? string.Empty;
	}

	public bool IsKind(TokenKind kind)
	{
		return Kind == kind;
	}

	public bool IsContextualKind(TokenKind kind)
	{
		return ContextualKind == kind;
	}
}
