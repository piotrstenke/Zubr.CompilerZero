using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Parser;
using Zubr.Compiler.Syntax;

namespace Zubr.Compiler;

public sealed class SyntaxTree
{
	private readonly DiagnosticMessage[]? _diagnostics;

	public CompilationUnitSyntax Root { get; }

	public Encoding Encoding { get; }

	public string? SourcePath { get; }

	public bool HasDiagnostics => _diagnostics is not null && _diagnostics.Length > 0;

	internal SyntaxTree(
		CompilationUnitSyntax root,
		string? sourcePath,
		Encoding encoding,
		DiagnosticMessage[]? diagnostics
	)
	{
		Root = root;
		SourcePath = sourcePath;
		Encoding = encoding;
		_diagnostics = diagnostics;
	}

	public DiagnosticMessage[] GetDiagnostics()
	{
		return _diagnostics ?? Array.Empty<DiagnosticMessage>();
	}

	public static SyntaxTree Parse(SourceText source)
	{
		SourceReader reader = source.GetSourceReader();
		Lexer lexer = new(reader);

		List<Token> tokens = new(source.Length * 2);

		while (true)
		{
			Token token = lexer.Lex();

			tokens.Add(token);

			if (token.IsKind(TokenKind.EOF))
			{
				break;
			}
		}

		List<InternalDiagnostic>? diagnostics = lexer.GetErrors();

		SourceParser parser = new(tokens.ToArray(), diagnostics);
		CompilationUnitSyntax root = parser.ParseCompilationUnit();

		diagnostics = parser.GetDiagnostics();

		return new(root, source.SourcePath, source.Encoding, GetDiagnostics(diagnostics, source.SourcePath));
	}

	[return: NotNullIfNotNull(nameof(diagnostics))]
	private static DiagnosticMessage[]? GetDiagnostics(List<InternalDiagnostic>? diagnostics, string? sourcePath)
	{
		if (diagnostics is null)
		{
			return null;
		}

		DiagnosticMessage[] array = new DiagnosticMessage[diagnostics.Count];

		for (int i = 0; i < diagnostics.Count; i++)
		{
			InternalDiagnostic diag = diagnostics[i];

			array[i] = new DiagnosticMessage(
				((int)diag.Code).ToString(),
				diag.Position,
				KnownDiagnostics.GetMessage(diag.Code),
				DiagnosticSeverity.Error,
				sourcePath
			);
		}

		return array;
	}
}
