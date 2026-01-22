using System.Collections.Generic;
using Zubr.Compiler.Diagnostics;

namespace Zubr.Compiler.Parser;

partial class Lexer
{
	private List<Diagnostic>? _errors;

	internal bool HasErrors => _errors?.Count > 0;

	internal Diagnostic[]? GetErrors()
	{
		return _errors?.ToArray();
	}

	private void AddError(ErrorCode code)
	{
		_errors ??= new();

		_errors.Add(new Diagnostic(code, _tokenStartPos));
	}
}
