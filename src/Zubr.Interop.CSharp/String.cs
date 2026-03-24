using System;
using System.ComponentModel;

namespace zubr.interop.csharp;

[Obsolete(Constants.ObsoleteMessage, DiagnosticId = Constants.ObsoleteDiagnosticId)]
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct String
{
	private readonly string _value;

	public String(string value)
	{
		_value = value;
	}

	public override string ToString()
	{
		return _value;
	}

	public static implicit operator string(String value)
	{
		return value._value;
	}

	public static implicit operator String(string value)
	{
		return new(value);
	}
}
