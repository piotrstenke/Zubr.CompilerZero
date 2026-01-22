using System.Collections.Generic;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Parser;

internal sealed class SourceParser
{
	private readonly SyntaxToken[] _tokens;
	private int _current;

	private List<Diagnostic>? _errors;

	public SourceParser(SyntaxToken[] tokens)
	{
		_tokens = tokens;
	}

	internal Diagnostic[]? GetDiagnostics()
	{
		return _errors?.ToArray();
	}

	public CompilationUnitSyntax ParseCompilationUnit()
	{
		SyntaxToken token;

		List<UseDirectiveSyntax> uses = new();
		List<MemberDeclarationSyntax> members = new();

		while(!(token = Current()).IsKind(SyntaxKind.EOF))
		{
			switch(token.Kind)
			{
				case SyntaxKind.UseKeyword:
					uses.Add(ParseUseDirective());
					break;

				case SyntaxKind.ModuleKeyword:
					members.Add(ParseModuleDeclaration());
					break;

				default:
					Move();
					break;
			}
		}

		return new(List(uses), List(members), token);
	}

	private ModuleDeclarationSyntax ParseModuleDeclaration()
	{
		SyntaxToken moduleKeyword = EatToken();
		SyntaxToken topKeyword;

		NameSyntax? name;

		SyntaxToken semicolonToken;

		if(PeekKind(SyntaxKind.TopKeyword))
		{
			name = null;

			topKeyword = EatToken();
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		}
		else
		{
			topKeyword = default;

			name = ParseName();
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		}

		return new(moduleKeyword, topKeyword, name, semicolonToken)
		{
			Position = moduleKeyword.Position
		};
	}

	private UseDirectiveSyntax ParseUseDirective()
	{
		SyntaxToken useKeyword = EatToken();

		NameSyntax name = ParseName();

		SyntaxToken asKeyword = default;
		IdentifierNameSyntax? alias = null;

		if (PeekKind(SyntaxKind.AsKeyword))
		{
			asKeyword = EatToken();
			alias = ParseIdentifierName();
		}

		SyntaxToken semicolon = EatToken(SyntaxKind.SemicolonToken);

		return new(useKeyword, name, asKeyword, alias, semicolon)
		{
			Position = useKeyword.Position
		};
	}

	private NameSyntax ParseName()
	{
		NameSyntax name = ParseIdentifierName();

		while(PeekKind(SyntaxKind.DotToken))
		{
			SyntaxToken dot = EatToken();

			IdentifierNameSyntax right = ParseIdentifierName();
			name = new QualifiedNameSyntax(name, dot, right)
			{
				Position = name.Position
			};
		}

		return name;
	}

	private IdentifierNameSyntax ParseIdentifierName()
	{
		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

		return new(identifier)
		{
			Position = identifier.Position
		};
	}

	private static SyntaxList<TNode> List<TNode>(List<TNode> nodes) where TNode : SyntaxNode
	{
		return new(nodes.ToArray());
	}

	private static SyntaxList<TNode> List<TNode>(TNode[] nodes) where TNode : SyntaxNode
	{
		return new(nodes);
	}

	private SyntaxToken EatToken(SyntaxKind kind)
	{
		ref readonly SyntaxToken current = ref EatToken();

		if(current.Kind != kind)
		{
			AddError(ErrorCode.ERR_SyntaxError);
			return MissingToken(current);
		}

		return current;
	}

	private ref readonly SyntaxToken EatToken()
	{
		return ref _tokens[_current++];
	}

	private ref readonly SyntaxToken Current()
	{
		return ref _tokens[_current];
	}

	private void Move()
	{
		_current++;
	}

	private bool PeekKind(SyntaxKind kind)
	{
		ref readonly SyntaxToken token = ref Current();
		return token.Kind == kind;
	}

	private static SyntaxToken MissingToken(in SyntaxToken current)
	{
		return new(SyntaxKind.MissingToken, string.Empty, current.Position);
	}

	private SyntaxToken UnexpectedToken()
	{
		ref readonly SyntaxToken current = ref EatToken();

		return new(SyntaxKind.BadToken, current.Text, current.Position);
	}

	private void AddError(ErrorCode code)
	{
		_errors ??= new();
		_errors.Add(new(code, _tokens[_current].Position));
	}
}
