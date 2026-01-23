using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Xml.Linq;
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

		while(!(token = Peek()).IsKind(SyntaxKind.EOF))
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

					if(TryParseMemberDeclaration() is MemberDeclarationSyntax member)
					{
						members.Add(member);
						break;
					}

					EatToken();
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

		List<MemberDeclarationSyntax> members = new();

		while (Peek().IsValid())
		{
			if (TryParseMemberDeclaration() is MemberDeclarationSyntax member)
			{
				members.Add(member);
			}
			else
			{
				break;
			}
		}

		return new(moduleKeyword, topKeyword, name, semicolonToken, List(members))
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

	private MemberDeclarationSyntax? TryParseMemberDeclaration()
	{
		SyntaxTokenList modifiers = ParseModifiers();

		while (true)
		{
			SyntaxToken token = Peek();

			if(token.IsPredefinedTypeKeyword())
			{
				return ParseFunctionDeclaration(modifiers);
			}

			return token.Kind switch
			{
				SyntaxKind.ClassKeyword 
					=> ParseClassDeclaration(modifiers),

				SyntaxKind.StructKeyword
					=> ParseStructDeclaration(modifiers),

				SyntaxKind.IdentifierName
					=> ParseFunctionDeclaration(modifiers),

				_ => null,
			};
		}
	}

	private FunctionDeclarationSyntax ParseFunctionDeclaration(SyntaxTokenList modifiers)
	{
		TypeSyntax returnType = ParseType();

		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

		ParameterListSyntax parameterList = ParseParameterList();

		SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);
		SyntaxToken closeBrace = EatToken(SyntaxKind.CloseBraceToken);

		BlockSyntax body = new(openBrace, default, closeBrace)
		{
			Position = openBrace.Position
		};

		return new(modifiers, returnType, identifier, parameterList, body)
		{
			Position = returnType.Position
		};
	}

	private ParameterListSyntax ParseParameterList()
	{
		SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);

		List<(ParameterSyntax parameter, SyntaxToken separator)> parameters = new();

		SyntaxToken token = Peek();

		if (token.IsKind(SyntaxKind.CloseParenToken))
		{
			EatToken();
		}
		else
		{
			while (token.IsValid())
			{
				ParameterSyntax parameter = ParseParameter();

				token = Peek();

				if (token.IsKind(SyntaxKind.CloseParenToken))
				{
					EatToken();
					parameters.Add((parameter, default));

					break;
				}

				if (token.IsKind(SyntaxKind.CommaToken))
				{
					EatToken();
					parameters.Add((parameter, token));

					continue;
				}
			}
		}

		return new(openParen, List(parameters), token)
		{
			Position = openParen.Position
		};
	}

	private ParameterSyntax ParseParameter()
	{
		List<SyntaxToken> modifiers = new();

		if(PeekKind(SyntaxKind.MutKeyword))
		{
			modifiers.Add(EatToken());
		}

		TypeSyntax type = ParseType();

		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

		EqualsValueClauseSyntax? @default = null;

		if(PeekKind(SyntaxKind.EqualsToken))
		{
			SyntaxToken equalsToken = EatToken();

			ExpressionSyntax value = ParseExpression();

			@default = new(equalsToken, value)
			{
				Position = equalsToken.Position
			};
		}

		return new(List(modifiers), type, identifier, @default)
		{
			Position = identifier.Position
		};
	}

	private ClassDeclarationSyntax ParseClassDeclaration(SyntaxTokenList modifiers)
	{
		return (ClassDeclarationSyntax)ParseTypeDeclaration(modifiers, SyntaxKind.ClassDeclaration);
	}

	private StructDeclarationSyntax ParseStructDeclaration(SyntaxTokenList modifiers)
	{
		foreach (SyntaxToken modifier in modifiers)
		{
			if(!modifier.IsAccessModifier())
			{
				AddError(ErrorCode.ERR_InvalidModifier, modifier.Position);
			}
		}

		return (StructDeclarationSyntax)ParseTypeDeclaration(modifiers, SyntaxKind.StructDeclaration);
	}

	private TypeDeclarationSyntax ParseTypeDeclaration(SyntaxTokenList modifiers, SyntaxKind kind)
	{
		SyntaxToken keyword = EatToken();

		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

		SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);

		List<MemberDeclarationSyntax> members = new();

		SyntaxToken token;

		while ((token = Peek()).IsValid() && !token.IsKind(SyntaxKind.CloseBraceToken))
		{
			if (TryParseMemberDeclaration() is MemberDeclarationSyntax member)
			{
				members.Add(member);
			}
		}

		SyntaxToken closeBrace = EatToken(SyntaxKind.CloseBraceToken);

		return kind switch
		{
			SyntaxKind.ClassDeclaration => new ClassDeclarationSyntax(modifiers, keyword, identifier, openBrace, List(members), closeBrace)
			{
				Position = keyword.Position
			},

			SyntaxKind.StructDeclaration => new StructDeclarationSyntax(modifiers, keyword, identifier, openBrace, List(members), closeBrace)
			{
				Position = keyword.Position
			},

			_ => throw new UnreachableException()
		};
	}

	private SyntaxTokenList ParseModifiers()
	{
		SyntaxToken token;

		List<SyntaxToken> modifiers = new();

		while ((token = Peek()).IsModifier())
		{
			EatToken();

			modifiers.Add(token);
		}

		return List(modifiers);
	}

	private ExpressionSyntax ParseExpression()
	{
		SyntaxToken token = EatToken();

		switch(token.Kind)
		{
			case SyntaxKind.StringLiteralToken:
				return new LiteralExpressionSyntax(SyntaxKind.StringLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.CharLiteralToken:
				return new LiteralExpressionSyntax(SyntaxKind.CharLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.NumericLiteralToken:
				return new LiteralExpressionSyntax(SyntaxKind.NumericLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.TrueKeyword:
				return new LiteralExpressionSyntax(SyntaxKind.TrueLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.FalseKeyword:
				return new LiteralExpressionSyntax(SyntaxKind.FalseLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.EOF:
				AddError(ErrorCode.ERR_UnexpectedEndOfFile, token.Position);

				return new LiteralExpressionSyntax(SyntaxKind.BadToken, UnexpectedToken())
				{
					Position = token.Position
				};

			default:
				AddError(ErrorCode.ERR_UnexpectedCharacter, token.Position);

				return new LiteralExpressionSyntax(SyntaxKind.BadToken, UnexpectedToken())
				{
					Position = token.Position
				};
		}
	}

	private TypeSyntax ParseType()
	{
		SyntaxToken token = Peek();

		if(token.IsPredefinedTypeKeyword())
		{
			SyntaxToken predefinedType = EatToken();

			return new PredefinedTypeSyntax(predefinedType)
			{
				Position = predefinedType.Position
			};
		}
		else
		{
			return ParseName();
		}
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

	private static SyntaxTokenList List(List<SyntaxToken> tokens)
	{
		return new(tokens.ToArray());
	}

	private static SeparatedSyntaxList<TNode> List<TNode>(List<(TNode node, SyntaxToken seprator)> nodes) where TNode : SyntaxNode
	{
		return new(nodes.ToArray());
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

	private ref readonly SyntaxToken Peek()
	{
		return ref _tokens[_current];
	}

	private bool PeekKind(SyntaxKind kind)
	{
		ref readonly SyntaxToken token = ref Peek();
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

	private void AddError(ErrorCode code, int position)
	{
		_errors ??= new();
		_errors.Add(new(code, position));
	}
}
