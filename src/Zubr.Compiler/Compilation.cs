using System.Collections.Generic;
using System.Linq;
using Zubr.Compiler.Diagnostics;

namespace Zubr.Compiler;

public sealed class Compilation
{
	private DiagnosticMessage[]? _diagnostics;
	private bool _hasCheckedDiagnostics;
	private bool _hasErrors;

	public string AssemblyName { get; }

	public LanguageVersion LanguageVersion { get; }

	public OutputKind OutputKind { get; }

	public IEnumerable<SyntaxTree> SyntaxTrees { get; }

	public bool HasDiagnostics
	{
		get
		{
			if(_diagnostics is not null)
			{
				return true;
			}

			if(_hasCheckedDiagnostics)
			{
				return false;
			}

			InitDiagnostics();

			return _diagnostics is not null;
		}
	}

	public bool HasErrors
	{
		get
		{
			if(_hasErrors)
			{
				return true;
			}

			if(_hasCheckedDiagnostics)
			{
				return false;
			}

			InitDiagnostics();
			return _hasErrors;
		}
	}

	internal Compilation(
		string assemblyName,
		OutputKind outputKind,
		LanguageVersion languageVersion,
		IEnumerable<SyntaxTree> syntaxTrees
	)
	{
		AssemblyName = assemblyName;
		SyntaxTrees = syntaxTrees;
		OutputKind = outputKind;
		LanguageVersion = languageVersion;
	}

	public DiagnosticMessage[] GetDiagnostics()
	{
		if(_diagnostics is not null)
		{
			return _diagnostics.ToArray();
		}

		DiagnosticMessage[] diagnostics = FetchDiagnostics();

		if (diagnostics.Length == 0)
		{
			return diagnostics;
		}

		_diagnostics = diagnostics;
		return _diagnostics.ToArray();
	}

	public static Compilation Create(
		string assemblyName,
		LanguageVersion languageVersion,
		OutputKind outputKind,
		IEnumerable<SyntaxTree> syntaxTrees
	)
	{
		return new Compilation(assemblyName, outputKind, languageVersion, syntaxTrees);
	}

	private void InitDiagnostics()
	{
		DiagnosticMessage[] diagnostics = FetchDiagnostics();

		if(diagnostics.Length == 0)
		{
			return;
		}

		_diagnostics = diagnostics;
	}

	private DiagnosticMessage[] FetchDiagnostics()
	{
		DiagnosticMessage[] diagnostics = SyntaxTrees
			.Where(x => x.HasDiagnostics)
			.SelectMany(x => x.GetDiagnostics())
			.ToArray();

		_hasCheckedDiagnostics = true;
		_hasErrors = diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

		return diagnostics;
	}
}
