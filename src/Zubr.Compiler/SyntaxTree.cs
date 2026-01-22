using Zubr.Compiler.Parser;
using System.Collections.Generic;
using Zubr.Compiler.Syntax;
using System.Text;

namespace Zubr.Compiler;

public sealed class SyntaxTree
{
	public CompilationUnitSyntax Root { get; }

	public Encoding Encoding { get; }

	internal SyntaxTree(CompilationUnitSyntax root, Encoding encoding)
	{
		Root = root;
		Encoding = encoding;
	}

	public static SyntaxTree Parse(SourceText source)
	{
		SourceReader reader = source.GetSourceReader();
		Lexer lexer = new(reader);

		List<SyntaxToken> tokens = new(source.Length * 2);
		SyntaxToken token;

		while ((token = lexer.Lex()).Kind != SyntaxKind.EOF)
		{
			tokens.Add(token);
		}

		// Add EOF
		tokens.Add(token);

		SourceParser parser = new(tokens.ToArray());
		CompilationUnitSyntax root = parser.ParseCompilationUnit();

		return new(root, source.Encoding);
	}
}
