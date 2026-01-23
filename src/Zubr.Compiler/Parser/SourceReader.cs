namespace Zubr.Compiler.Parser;

internal sealed class SourceReader
{
	public const char InvalidChar = char.MaxValue;

	public SourceText Source { get; }

	private int _pos;

	private readonly int _length;

	public int Position => _pos;

	public SourceReader(SourceText source)
	{
		Source = source;
		_length = source.Length;
	}

	public bool IsEnd()
	{
		return _pos >= _length;
	}

	public bool IsValid(int pos)
	{
		return pos < _length;
	}

	public char Peek()
	{
		if(IsEnd())
		{
			return InvalidChar;
		}

		return Source[_pos];
	}

	public char Peek(int dist)
	{
		int target = _pos + dist;

		if(!IsValid(target))
		{
			return InvalidChar;
		}

		return Source[target];
	}

	public char Read()
	{
		if (IsEnd())
		{
			return InvalidChar;
		}

		return Source[_pos++];
	}

	public void Move()
	{
		_pos++;
	}

	public void Move(int dist)
	{
		_pos += dist;
	}

	public char MovePeek()
	{
		_pos++;

		if (IsEnd())
		{
			return InvalidChar;
		}

		return Source[_pos];
	}

	public bool TryMove()
	{
		if (IsEnd())
		{
			return false;
		}

		_pos++;
		return true;
	}

	public void Reset(int pos)
	{
		_pos = pos;
	}
}
