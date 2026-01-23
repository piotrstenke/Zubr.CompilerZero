using System.Collections.Generic;
using System.Diagnostics;
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

		TypeParameterListSyntax? typeParameterList = TryParseTypeParameterList();
		ParameterListSyntax parameterList = ParseParameterList();
		TypeParameterConstraintListSyntax? constraintList = TryParseConstraintList();

		BlockSyntax body = ParseBlock();

		return new(modifiers, returnType, identifier, typeParameterList, parameterList, constraintList, body)
		{
			Position = returnType.Position
		};
	}

	private TypeParameterListSyntax? TryParseTypeParameterList()
	{
		SyntaxToken lessThanToken = Peek();

		if(!lessThanToken.IsKind(SyntaxKind.LessThanToken))
		{
			return null;
		}

		EatToken();

		List<(TypeParameterSyntax, SyntaxToken)> parameters = new();

		while(true)
		{
			SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

			SyntaxToken token = Peek();

			TypeParameterSyntax typeParameter = new(identifier)
			{
				Position = identifier.Position
			};

			if (token.IsKind(SyntaxKind.GreaterThanToken))
			{
				parameters.Add((typeParameter, default));
				break;
			}

			EatToken(SyntaxKind.CommaToken);

			parameters.Add((typeParameter, token));
		}

		SyntaxToken greaterThanToken = EatToken();

		return new(lessThanToken, List(parameters), greaterThanToken);
	}

	private TypeParameterConstraintListSyntax? TryParseConstraintList()
	{
		SyntaxToken whereKeyword = Peek();

		if(!whereKeyword.IsKind(SyntaxKind.WhereKeyword))
		{
			return null;
		}

		EatToken();

		List<(TypeParameterConstraintClauseSyntax, SyntaxToken)> clauses = new();

		while (true)
		{
			SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);
			SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);

			List<(TypeParameterConstraintSyntax, SyntaxToken)> constraints = new();
			
			// Comma token must be default-initialized, because the child constraint clause might not have a comma.
			SyntaxToken commaToken = default;

			while (true)
			{
				if(TryParseConstraint(Peek()) is not TypeParameterConstraintSyntax constraint)
				{
					break;
				}

				// This is the last constraint, so comma is not required.
				if(!Peek().IsKind(SyntaxKind.CommaToken))
				{
					constraints.Add((constraint, default));
					break;
				}

				commaToken = EatToken();

				// If the next token is an identifier, it could mean either:
				//
				// 1. Another type constraint for the current constraint clause.
				// 2. Identifier of type parameter in the next constraint clause.
				if(!Peek().IsKind(SyntaxKind.IdentifierToken))
				{
					// Not an identifier, so we can go directly to the next constraint.
					constraints.Add((constraint, commaToken));
					continue;
				}

				// Scenario 1, so add the constraint with comma and go to the next constraint.
				if(!Peek(1).IsKind(SyntaxKind.ColonToken))
				{
					constraints.Add((constraint, commaToken));
					continue;
				}

				// Scenario 2, the comma belongs to the parent constraint clause, not the constraint itself.
				constraints.Add((constraint, default));
				break;
			}

			TypeParameterConstraintClauseSyntax clause = new(identifier, colonToken, List(constraints))
			{
				Position = identifier.Position
			};

			clauses.Add((clause, commaToken));

			// End of the constraint clause list.
			if(!Peek().IsKind(SyntaxKind.IdentifierToken))
			{
				break;
			}
		}

		return new(whereKeyword, List(clauses))
		{
			Position = whereKeyword.Position,
		};
	}

	private TypeParameterConstraintSyntax? TryParseConstraint(SyntaxToken token)
	{
		switch(token.Kind)
		{
			case SyntaxKind.ClassKeyword:
				EatToken();

				return new ClassConstraintSyntax(token)
				{
					Position = token.Position
				};

			case SyntaxKind.StructKeyword:
				EatToken();

				return new StructConstraintSyntax(token)
				{
					Position = token.Position
				};

			case SyntaxKind.IdentifierToken:
				return new TypeConstraintSyntax(ParseName())
				{
					Position = token.Position
				};

			default:
				return null;
		}
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

		TypeParameterListSyntax? typeParameterList = TryParseTypeParameterList();
		TypeParameterConstraintListSyntax? constraints = TryParseConstraintList();

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
			SyntaxKind.ClassDeclaration => new ClassDeclarationSyntax(modifiers, keyword, identifier, typeParameterList, constraints, openBrace, List(members), closeBrace)
			{
				Position = keyword.Position
			},

			SyntaxKind.StructDeclaration => new StructDeclarationSyntax(modifiers, keyword, identifier, typeParameterList, constraints, openBrace, List(members), closeBrace)
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

	private BlockSyntax ParseBlock()
	{
		SyntaxToken openBrace = EatToken(SyntaxKind.OpenBraceToken);

		List<StatementSyntax> statements = new();

		while(true)
		{
			SyntaxToken token = Peek();

			if(token.IsKind(SyntaxKind.EOF) || token.IsKind(SyntaxKind.CloseBraceToken))
			{
				break;
			}

			statements.Add(ParseStatement());
		}

		SyntaxToken closeBrace = EatToken(SyntaxKind.CloseBraceToken);

		return new(openBrace, List(statements), closeBrace)
		{
			Position = openBrace.Position
		};
	}

	private StatementSyntax ParseStatement()
	{
		SyntaxToken token = Peek();

		switch(token.Kind)
		{
			case SyntaxKind.OpenBraceToken:
				return ParseBlock();

			case SyntaxKind.IfKeyword:
				return ParseIfStatement();

			case SyntaxKind.WhileKeyword:
				return ParseWhileStatement();

			case SyntaxKind.DoKeyword:
				return ParseDoStatement();

			case SyntaxKind.ForKeyword:
				return ParseForStatement();

			case SyntaxKind.IdentifierToken:
				return ParseLocalDeclaration();

			case SyntaxKind.ReturnKeyword:
				return ParseReturnStatement();

			case SyntaxKind.NextKeyword:
				return ParseNextStatement();

			case SyntaxKind.StopKeyword:
				return ParseStopStatement();

			default:
				return ParseLocalDeclaration();
		}
	}

	private ReturnStatementSyntax ParseReturnStatement()
	{
		SyntaxToken returnKeyword = EatToken(SyntaxKind.ReturnKeyword);

		ExpressionSyntax? expression = null;

		if (!PeekKind(SyntaxKind.SemicolonToken))
		{
			expression = ParseExpression();
		}

		SyntaxToken semicolon = EatToken(SyntaxKind.SemicolonToken);

		return new(returnKeyword, expression, semicolon)
		{
			Position = returnKeyword.Position,
		};
	}

	private NextStatementSyntax ParseNextStatement()
	{
		SyntaxToken nextKeyword = EatToken(SyntaxKind.NextKeyword);
		SyntaxToken semicolon = EatToken(SyntaxKind.SemicolonToken);

		return new(nextKeyword, semicolon)
		{
			Position = nextKeyword.Position,
		};
	}

	private StopStatementSyntax ParseStopStatement()
	{
		SyntaxToken stopKeyword = EatToken(SyntaxKind.StopKeyword);
		SyntaxToken semicolon = EatToken(SyntaxKind.SemicolonToken);

		return new(stopKeyword, semicolon)
		{
			Position = stopKeyword.Position,
		};
	}

	private LocalDeclarationStatementSyntax ParseLocalDeclaration()
	{
		TypeSyntax type = ParseType();

		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

		SyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);

		ExpressionSyntax expression = ParseExpression();

		EqualsValueClauseSyntax clause = new(equalsToken, expression)
		{
			Position = equalsToken.Position
		};

		VariableDeclaratorSyntax declarator = new(identifier, clause)
		{
			Position = identifier.Position
		};

		VariableDeclarationSyntax variable = new(type, declarator)
		{
			Position = type.Position
		};

		SyntaxToken semicolon = EatToken(SyntaxKind.SemicolonToken);

		if (type is PredefinedTypeSyntax p && p.Keyword.IsKind(SyntaxKind.VoidKeyword))
		{
			AddError(ErrorCode.ERR_SyntaxError);
		}

		return new(default, variable, semicolon)
		{
			Position = type.Position
		};
	}

	private WhileStatementSyntax ParseWhileStatement()
	{
		SyntaxToken whileKeyword = EatToken(SyntaxKind.WhileKeyword);
		SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		SyntaxToken closeParen = EatToken(SyntaxKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		return new(whileKeyword, openParen, condition, closeParen, statement)
		{
			Position = whileKeyword.Position
		};
	}

	private DoStatementSyntax ParseDoStatement()
	{
		SyntaxToken doKeyword = EatToken(SyntaxKind.DoKeyword);

		StatementSyntax statement = ParseStatement();

		SyntaxToken whileKeyword = EatToken(SyntaxKind.WhileKeyword);
		SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		SyntaxToken closeParen = EatToken(SyntaxKind.CloseParenToken);
		SyntaxToken semicolon = EatToken(SyntaxKind.SemicolonToken);

		return new(doKeyword, statement, whileKeyword, openParen, condition, closeParen, semicolon)
		{
			Position = doKeyword.Position
		};
	}

	private ForStatementSyntax ParseForStatement()
	{
		SyntaxToken forKeyword = EatToken(SyntaxKind.ForKeyword);

		SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);

		TypeSyntax type = ParseType();
		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

		VariableExpressionSyntax variable = new(type, identifier)
		{
			Position = type.Position
		};

		SyntaxToken colon = EatToken(SyntaxKind.ColonToken);

		ExpressionSyntax expression = ParseExpression();

		SyntaxToken closeParen = EatToken(SyntaxKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		return new(forKeyword, openParen, variable, colon, expression, closeParen, statement);
	}

	private IfStatementSyntax ParseIfStatement()
	{
		SyntaxToken ifKeyword = EatToken(SyntaxKind.IfKeyword);
		SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		SyntaxToken closeParen = EatToken(SyntaxKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		List<ElifClauseSyntax>? elifs = null;
		ElseClauseSyntax? @else = null;

		if(PeekKind(SyntaxKind.ElifKeyword))
		{
			elifs = new()
			{
				ParseElifClause()
			};

			while (PeekKind(SyntaxKind.ElifKeyword))
			{
				elifs.Add(ParseElifClause());
			}
		}

		if(PeekKind(SyntaxKind.ElseKeyword))
		{
			SyntaxToken elseKeyword = EatToken();

			StatementSyntax elseStatement = ParseStatement();

			if (elseStatement.IsKind(SyntaxKind.IfStatement))
			{
				AddError(ErrorCode.ERR_ElseIfNotSupported, statement.Position);
			}

			@else = new(elseKeyword, elseStatement)
			{
				Position = elseKeyword.Position
			};
		}

		return new(ifKeyword, openParen, condition, closeParen, statement, ListIfNotNull(elifs), @else)
		{
			Position = ifKeyword.Position
		};
	}

	private ElifClauseSyntax ParseElifClause()
	{
		SyntaxToken elifKeyword = EatToken(SyntaxKind.ElifKeyword);
		SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		SyntaxToken closeParen = EatToken(SyntaxKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		return new(elifKeyword, openParen, condition, closeParen, statement);
	}

	private ExpressionSyntax ParseExpression()
	{
		SyntaxToken token = Peek();

		switch(token.Kind)
		{
			case SyntaxKind.IdentifierToken:
				return ParseIdentifierName();

			case SyntaxKind.StringLiteralToken:
				EatToken();

				return new LiteralExpressionSyntax(SyntaxKind.StringLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.CharLiteralToken:
				EatToken();

				return new LiteralExpressionSyntax(SyntaxKind.CharLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.NumericLiteralToken:
				EatToken();

				return new LiteralExpressionSyntax(SyntaxKind.NumericLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.TrueKeyword:
				EatToken();

				return new LiteralExpressionSyntax(SyntaxKind.TrueLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.FalseKeyword:
				EatToken();

				return new LiteralExpressionSyntax(SyntaxKind.FalseLiteralExpression, token)
				{
					Position = token.Position
				};

			case SyntaxKind.EOF:
				EatToken();
				AddError(ErrorCode.ERR_UnexpectedEndOfFile, token.Position);

				return new LiteralExpressionSyntax(SyntaxKind.BadToken, UnexpectedToken(token))
				{
					Position = token.Position
				};

			default:
				EatToken();
				AddError(ErrorCode.ERR_SyntaxError, token.Position);

				return new LiteralExpressionSyntax(SyntaxKind.BadToken, UnexpectedToken(token))
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
			EatToken();

			return new PredefinedTypeSyntax(token)
			{
				Position = token.Position
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

	private static SyntaxList<TNode> ListIfNotNull<TNode>(List<TNode>? nodes) where TNode : SyntaxNode
	{
		if(nodes is null)
		{
			return default;
		}

		return List(nodes);
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
		ref readonly SyntaxToken current = ref Peek();

		if(current.Kind != kind)
		{
			if (current.Kind == SyntaxKind.EOF)
			{
				AddError(ErrorCode.ERR_UnexpectedEndOfFile);
			}
			else
			{
				AddError(ErrorCode.ERR_SyntaxError);
			}

			return MissingToken(current);
		}

		EatToken();

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

	private ref readonly SyntaxToken Peek(int pos)
	{
		int next = _current + pos;

		// Return EOF if pos is too big.
		if(next > _tokens.Length - 1)
		{
			return ref _tokens[^1];
		}

		return ref _tokens[_current + pos];
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
		return UnexpectedToken(in current);
	}

	private static SyntaxToken UnexpectedToken(in SyntaxToken token)
	{
		return new(SyntaxKind.BadToken, token.Text, token.Position);
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
