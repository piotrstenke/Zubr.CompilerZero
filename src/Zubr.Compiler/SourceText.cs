using System.Text;
using Zubr.Compiler.Parser;

namespace Zubr.Compiler;

public sealed class SourceText
{
	private readonly string _source;

	public Encoding Encoding { get; }

	public int Length => _source.Length;

	public char this[int index] => _source[index];

	private SourceText(string source, Encoding encoding)
	{
		_source = source;
		Encoding = encoding;
	}

	internal SourceReader GetSourceReader()
	{
		return new SourceReader(this);
	}

	public static SourceText FromSource(string source, Encoding? encoding = null)
	{
		return new SourceText(source, encoding ?? Encoding.UTF8);
	}
}
