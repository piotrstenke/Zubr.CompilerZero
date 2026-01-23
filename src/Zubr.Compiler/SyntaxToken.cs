using System.Diagnostics;

namespace Zubr.Compiler;

[DebuggerDisplay("{Text ?? string.Empty,nq}")]
public readonly struct SyntaxToken
{
	public object? Value { get; }

	public string Text { get; }

	public int Position { get; }

	public SyntaxKind Kind { get; }

	public int Length => Text.Length;

	public bool IsNone => Kind == SyntaxKind.None;

	public bool Exists => !IsNone;

	internal SyntaxToken(SyntaxKind kind, string text, int position)
	{
		Kind = kind;
		Text = text;
		Position = position;
	}

	internal SyntaxToken(SyntaxKind kind, string text, int position, object? value)
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
}
