using System;
using System.Diagnostics;

namespace Zubr.Compiler.Text;

[DebuggerDisplay("{ToString(),nq}")]
public readonly record struct Location
{
	public static Location Invalid => default;

	public string Path { get; }

	public TextSpan Span { get; }

	public int Line { get; }

	public int LinePosition { get; }

	public bool IsValid => Path is not null;

	internal Location(string path, TextSpan span, int line, int linePosition)
	{
		Path = path;
		Span = span;
		Line = line;
		LinePosition = linePosition;
	}

	public bool Equals(Location other)
	{
		return
			Path == other.Path &&
			Span == other.Span;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Path, Span);
	}

	public override string ToString()
	{
		if(!IsValid)
		{
			return "invalid";
		}

		return $"{Path}:{Line + 1},{LinePosition + 1}";
	}

	public Location MoveEnd(int end)
	{
		return new(Path, Span.MoveEnd(end), Line, LinePosition);
	}

	public static Location Create(string path, TextSpan span, int line, int linePosition)
	{
		return new(path, span, line, linePosition);
	}
}
