using System.Collections.Generic;
using System.Text;
using Zubr.Compiler.Parser;
using Zubr.Compiler.Syntax;

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

		SourceParser parser = new(tokens.ToArray());
		CompilationUnitSyntax root = parser.ParseCompilationUnit();

		return new(root, source.Encoding);
	}
}
