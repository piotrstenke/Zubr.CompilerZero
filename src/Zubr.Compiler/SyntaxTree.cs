using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Parser;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Text;

namespace Zubr.Compiler;

public sealed class SyntaxTree
{
	private DiagnosticMessage[]? _diagnostics;
	private readonly int[] _lineStartPositions;

	public SyntaxNode Root { get; private set; } = default!;

	public Encoding Encoding { get; }

	public string? SourcePath { get; }

	public bool HasDiagnostics => _diagnostics is not null && _diagnostics.Length > 0;

	public int Length => Root.Span.Length;

	private SyntaxTree(string? sourcePath, Encoding encoding, int[] lineStartPositions)
	{
		Encoding = encoding;
		SourcePath = sourcePath;
		_lineStartPositions = lineStartPositions;
	}

	public DiagnosticMessage[] GetDiagnostics()
	{
		return _diagnostics ?? Array.Empty<DiagnosticMessage>();
	}

	public Location GetLocation(int position)
	{
		return GetLocation(new TextSpan(position, position));
	}

	public Location GetLocation(TextSpan span)
	{
		int line = GetLine(span.Start);

		if(line == -1)
		{
			return Location.Invalid;
		}

		int linePosition = GetLinePosition(line, span.Start);

		return new(SourcePath ?? string.Empty, span, line, linePosition);
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
		SyntaxTree tree = new(source.SourcePath, source.Encoding, lexer.GetLineStartPositions());

		SourceParser parser = new(tree, tokens.ToArray(), diagnostics);
		CompilationUnitSyntax root = parser.ParseCompilationUnit();

		diagnostics = parser.GetDiagnostics();

		tree.AttachRoot(root, GetDiagnostics(diagnostics, tree));
		return tree;
	}

	private int GetLine(int position)
	{
		int i = 1;

		while (i < _lineStartPositions.Length)
		{
			if (_lineStartPositions[i] >= position)
			{
				return i - 1;
			}

			i++;
		}

		return -1;
	}

	private int GetLinePosition(int line, int absolutePosition)
	{
		int lineStart = _lineStartPositions[line];
		return absolutePosition - lineStart;
	}

	private void AttachRoot(
		CompilationUnitSyntax root,
		DiagnosticMessage[]? diagnostics
	)
	{
		Root = root;
		_diagnostics = diagnostics;
	}

	[return: NotNullIfNotNull(nameof(diagnostics))]
	private static DiagnosticMessage[]? GetDiagnostics(List<InternalDiagnostic>? diagnostics, SyntaxTree tree)
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
				KnownDiagnostics.GetMessage(diag.Code),
				DiagnosticSeverity.Error,
				tree.GetLocation(diag.Position)
			);
		}

		return array;
	}
}
