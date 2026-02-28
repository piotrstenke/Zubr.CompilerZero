using System;

namespace Zubr.Compiler.Text;

public readonly record struct TextSpan
{
	public int Start { get; }

	public int End => Start + Length;

	public int Length { get; }

	public bool IsEmpty => Length == 0;

	internal TextSpan(int start, int length)
	{
		Start = start;
		Length = length;
	}

	public override string ToString()
	{
		return $"{Start}-{End}";
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Start, Length);
	}

	public bool Equals(TextSpan other)
	{
		return other.Start == Start && other.Length == Length;
	}

	public TextSpan MoveEnd(int end)
	{
		return new(Start, end);
	}

	public TextSpan MoveEnd(TextSpan span)
	{
		return MoveEnd(span.End);
	}
}
