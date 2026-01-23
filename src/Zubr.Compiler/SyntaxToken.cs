namespace Zubr.Compiler;

public readonly struct SyntaxToken
{
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

	public override string ToString()
	{
		return Text ?? string.Empty;
	}
}
