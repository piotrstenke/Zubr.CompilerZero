using System;
using System.Collections.Generic;
using System.Diagnostics;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler.Parser;

internal sealed class SourceParser
{
	private readonly Token[] _tokens;
	private int _current;

	private List<InternalDiagnostic>? _errors;

	public SourceParser(Token[] tokens) : this(tokens, null)
	{
	}

	public SourceParser(Token[] tokens, List<InternalDiagnostic>? errors)
	{
		_tokens = tokens;
		_errors = errors;
	}

	public CompilationUnitSyntax ParseCompilationUnit()
	{
		Token token;

		List<UseDirectiveSyntax> uses = new();
		List<MemberDeclarationSyntax> members = new();

		while (!(token = Peek()).IsKind(TokenKind.EOF))
		{
			switch (token.Kind)
			{
				case TokenKind.UseKeyword:
					uses.Add(ParseUseDirective());
					break;

				case TokenKind.ModuleKeyword:
					members.Add(ParseModuleDeclaration());
					break;

				default:

					if (TryParseMemberDeclaration() is MemberDeclarationSyntax member)
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

	internal List<InternalDiagnostic>? GetDiagnostics()
	{
		return _errors;
	}

	private ModuleDeclarationSyntax ParseModuleDeclaration()
	{
		Token moduleKeyword = EatToken();
		Token topKeyword;

		NameSyntax? name;

		Token semicolonToken;

		if (PeekKind(TokenKind.TopKeyword))
		{
			name = null;

			topKeyword = EatToken();
			semicolonToken = EatToken(TokenKind.SemicolonToken);
		}
		else
		{
			topKeyword = default;

			name = ParseName();
			semicolonToken = EatToken(TokenKind.SemicolonToken);
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
		Token useKeyword = EatToken();

		NameSyntax name = ParseName();

		Token asKeyword = default;
		IdentifierNameSyntax? alias = null;

		if (PeekKind(TokenKind.AsKeyword))
		{
			asKeyword = EatToken();
			alias = ParseIdentifierName();
		}

		Token semicolon = EatToken(TokenKind.SemicolonToken);

		return new(useKeyword, name, asKeyword, alias, semicolon)
		{
			Position = useKeyword.Position
		};
	}

	private MemberDeclarationSyntax? TryParseMemberDeclaration()
	{
		TokenList modifiers = ParseModifiers();

		while (true)
		{
			Token token = Peek();

			if (token.IsPredefinedType())
			{
				return ParseFunctionDeclaration(modifiers);
			}

			return token.Kind switch
			{
				TokenKind.ClassKeyword
					=> ParseClassDeclaration(modifiers),

				TokenKind.StructKeyword
					=> ParseStructDeclaration(modifiers),

				TokenKind.IdentifierToken
					=> ParseFunctionDeclaration(modifiers),

				_ => null,
			};
		}
	}

	private FunctionDeclarationSyntax ParseFunctionDeclaration(TokenList modifiers)
	{
		TypeSyntax returnType = ParseType();

		Token identifier = EatToken(TokenKind.IdentifierToken);

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
		Token lessThanToken = Peek();

		if (!lessThanToken.IsKind(TokenKind.LessThanToken))
		{
			return null;
		}

		EatToken();

		List<(TypeParameterSyntax, Token)> parameters = new();

		while (true)
		{
			Token identifier = EatToken(TokenKind.IdentifierToken);

			Token token = Peek();

			TypeParameterSyntax typeParameter = new(identifier)
			{
				Position = identifier.Position
			};

			if (token.IsKind(TokenKind.GreaterThanToken))
			{
				parameters.Add((typeParameter, default));
				break;
			}

			EatToken(TokenKind.CommaToken);

			parameters.Add((typeParameter, token));
		}

		Token greaterThanToken = EatToken();

		return new(lessThanToken, List(parameters), greaterThanToken);
	}

	private TypeParameterConstraintListSyntax? TryParseConstraintList()
	{
		Token whereKeyword = Peek();

		if (!whereKeyword.IsKind(TokenKind.WhereKeyword))
		{
			return null;
		}

		EatToken();

		List<(TypeParameterConstraintClauseSyntax, Token)> clauses = new();

		while (true)
		{
			Token identifier = EatToken(TokenKind.IdentifierToken);
			Token colonToken = EatToken(TokenKind.ColonToken);

			List<(TypeParameterConstraintSyntax, Token)> constraints = new();

			// Comma token must be default-initialized, because the child constraint clause might not have a comma.
			Token commaToken = default;

			while (true)
			{
				if (TryParseConstraint(Peek()) is not TypeParameterConstraintSyntax constraint)
				{
					break;
				}

				// This is the last constraint, so comma is not required.
				if (!Peek().IsKind(TokenKind.CommaToken))
				{
					constraints.Add((constraint, default));
					break;
				}

				commaToken = EatToken();

				// If the next token is an identifier, it could mean either:
				//
				// 1. Another type constraint for the current constraint clause.
				// 2. Identifier of type parameter in the next constraint clause.
				if (!Peek().IsKind(TokenKind.IdentifierToken))
				{
					// Not an identifier, so we can go directly to the next constraint.
					constraints.Add((constraint, commaToken));
					continue;
				}

				// Scenario 1, so add the constraint with comma and go to the next constraint.
				if (!Peek(1).IsKind(TokenKind.ColonToken))
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
			if (!Peek().IsKind(TokenKind.IdentifierToken))
			{
				break;
			}
		}

		return new(whereKeyword, List(clauses))
		{
			Position = whereKeyword.Position,
		};
	}

	private TypeParameterConstraintSyntax? TryParseConstraint(Token token)
	{
		switch (token.Kind)
		{
			case TokenKind.ClassKeyword:
				EatToken();

				return new ClassConstraintSyntax(token)
				{
					Position = token.Position
				};

			case TokenKind.StructKeyword:
				EatToken();

				return new StructConstraintSyntax(token)
				{
					Position = token.Position
				};

			case TokenKind.IdentifierToken:
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
		Token openParen = EatToken(TokenKind.OpenParenToken);

		List<(ParameterSyntax parameter, Token separator)> parameters = new();

		Token token = Peek();

		if (token.IsKind(TokenKind.CloseParenToken))
		{
			EatToken();
		}
		else
		{
			while (token.IsValid())
			{
				ParameterSyntax parameter = ParseParameter();

				token = Peek();

				if (token.IsKind(TokenKind.CloseParenToken))
				{
					EatToken();
					parameters.Add((parameter, default));

					break;
				}

				if (token.IsKind(TokenKind.CommaToken))
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
		List<Token> modifiers = new();

		if (PeekKind(TokenKind.MutKeyword))
		{
			modifiers.Add(EatToken());
		}

		TypeSyntax type = ParseType();

		Token identifier = EatToken(TokenKind.IdentifierToken);

		EqualsValueClauseSyntax? @default = null;

		if (PeekKind(TokenKind.EqualsToken))
		{
			Token equalsToken = EatToken();

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

	private ClassDeclarationSyntax ParseClassDeclaration(TokenList modifiers)
	{
		return (ClassDeclarationSyntax)ParseTypeDeclaration(modifiers, SyntaxKind.ClassDeclaration);
	}

	private StructDeclarationSyntax ParseStructDeclaration(TokenList modifiers)
	{
		foreach (Token modifier in modifiers)
		{
			if (!modifier.IsAccessModifier())
			{
				AddError(ErrorCode.ERR_InvalidModifier, modifier.Position);
			}
		}

		return (StructDeclarationSyntax)ParseTypeDeclaration(modifiers, SyntaxKind.StructDeclaration);
	}

	private TypeDeclarationSyntax ParseTypeDeclaration(TokenList modifiers, SyntaxKind kind)
	{
		Token keyword = EatToken();

		Token identifier = EatToken(TokenKind.IdentifierToken);

		TypeParameterListSyntax? typeParameterList = TryParseTypeParameterList();
		TypeParameterConstraintListSyntax? constraints = TryParseConstraintList();

		Token openBrace = EatToken(TokenKind.OpenBraceToken);

		List<MemberDeclarationSyntax> members = new();

		Token token;

		while ((token = Peek()).IsValid() && !token.IsKind(TokenKind.CloseBraceToken))
		{
			if (TryParseMemberDeclaration() is MemberDeclarationSyntax member)
			{
				members.Add(member);
			}
		}

		Token closeBrace = EatToken(TokenKind.CloseBraceToken);

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

	private TokenList ParseModifiers()
	{
		Token token;

		List<Token> modifiers = new();

		while ((token = Peek()).IsModifier())
		{
			EatToken();

			modifiers.Add(token);
		}

		return List(modifiers);
	}

	private BlockSyntax ParseBlock()
	{
		Token openBrace = EatToken(TokenKind.OpenBraceToken);

		List<StatementSyntax> statements = new();

		while (true)
		{
			Token token = Peek();

			if (token.IsKind(TokenKind.EOF) || token.IsKind(TokenKind.CloseBraceToken))
			{
				break;
			}

			statements.Add(ParseStatement());
		}

		Token closeBrace = EatToken(TokenKind.CloseBraceToken);

		return new(openBrace, List(statements), closeBrace)
		{
			Position = openBrace.Position
		};
	}

	private StatementSyntax ParseStatement()
	{
		Token token = Peek();

		return token.Kind switch
		{
			TokenKind.OpenBraceToken => ParseBlock(),
			TokenKind.IfKeyword => ParseIfStatement(),
			TokenKind.WhileKeyword => ParseWhileStatement(),
			TokenKind.DoKeyword => ParseDoStatement(),
			TokenKind.ForKeyword => ParseForStatement(),
			TokenKind.IdentifierToken => ParseLocalOrExpressionStatement(),
			TokenKind.ReturnKeyword => ParseReturnStatement(),
			TokenKind.NextKeyword => ParseNextStatement(),
			TokenKind.StopKeyword => ParseStopStatement(),
			_ => ParseLocalOrExpressionStatement(),
		};
	}

	private ReturnStatementSyntax ParseReturnStatement()
	{
		Token returnKeyword = EatToken(TokenKind.ReturnKeyword);

		ExpressionSyntax? expression = null;

		if (!PeekKind(TokenKind.SemicolonToken))
		{
			expression = ParseExpression();
		}

		Token semicolon = EatToken(TokenKind.SemicolonToken);

		return new(returnKeyword, expression, semicolon)
		{
			Position = returnKeyword.Position,
		};
	}

	private NextStatementSyntax ParseNextStatement()
	{
		Token nextKeyword = EatToken(TokenKind.NextKeyword);
		Token semicolon = EatToken(TokenKind.SemicolonToken);

		return new(nextKeyword, semicolon)
		{
			Position = nextKeyword.Position,
		};
	}

	private StopStatementSyntax ParseStopStatement()
	{
		Token stopKeyword = EatToken(TokenKind.StopKeyword);
		Token semicolon = EatToken(TokenKind.SemicolonToken);

		return new(stopKeyword, semicolon)
		{
			Position = stopKeyword.Position,
		};
	}

	private StatementSyntax ParseLocalOrExpressionStatement()
	{
		Token token = Peek();

		if (token.IsPredefinedType() && Peek(1).Kind != TokenKind.DotToken)
		{
			return ParseLocalDeclaration();
		}

		ExpressionSyntax expr = ParseExpression();
		Token semicolonToken = EatToken();

		return new ExpressionStatementSyntax(expr, semicolonToken)
		{
			Position = expr.Position,
		};
	}

	private LocalDeclarationStatementSyntax ParseLocalDeclaration()
	{
		TypeSyntax type = ParseType();

		Token identifier = EatToken(TokenKind.IdentifierToken);

		Token equalsToken = EatToken(TokenKind.EqualsToken);

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

		Token semicolon = EatToken(TokenKind.SemicolonToken);

		if (type is PredefinedTypeSyntax p && p.Keyword.IsKind(TokenKind.VoidKeyword))
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
		Token whileKeyword = EatToken(TokenKind.WhileKeyword);
		Token openParen = EatToken(TokenKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		Token closeParen = EatToken(TokenKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		return new(whileKeyword, openParen, condition, closeParen, statement)
		{
			Position = whileKeyword.Position
		};
	}

	private DoStatementSyntax ParseDoStatement()
	{
		Token doKeyword = EatToken(TokenKind.DoKeyword);

		StatementSyntax statement = ParseStatement();

		Token whileKeyword = EatToken(TokenKind.WhileKeyword);
		Token openParen = EatToken(TokenKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		Token closeParen = EatToken(TokenKind.CloseParenToken);
		Token semicolon = EatToken(TokenKind.SemicolonToken);

		return new(doKeyword, statement, whileKeyword, openParen, condition, closeParen, semicolon)
		{
			Position = doKeyword.Position
		};
	}

	private ForStatementSyntax ParseForStatement()
	{
		Token forKeyword = EatToken(TokenKind.ForKeyword);

		Token openParen = EatToken(TokenKind.OpenParenToken);

		TypeSyntax type = ParseType();
		Token identifier = EatToken(TokenKind.IdentifierToken);

		VariableExpressionSyntax variable = new(type, identifier)
		{
			Position = type.Position
		};

		Token colon = EatToken(TokenKind.ColonToken);

		ExpressionSyntax expression = ParseExpression();

		Token closeParen = EatToken(TokenKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		return new(forKeyword, openParen, variable, colon, expression, closeParen, statement);
	}

	private IfStatementSyntax ParseIfStatement()
	{
		Token ifKeyword = EatToken(TokenKind.IfKeyword);
		Token openParen = EatToken(TokenKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		Token closeParen = EatToken(TokenKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		List<ElifClauseSyntax>? elifs = null;
		ElseClauseSyntax? @else = null;

		if (PeekKind(TokenKind.ElifKeyword))
		{
			elifs = new()
			{
				ParseElifClause()
			};

			while (PeekKind(TokenKind.ElifKeyword))
			{
				elifs.Add(ParseElifClause());
			}
		}

		if (PeekKind(TokenKind.ElseKeyword))
		{
			Token elseKeyword = EatToken();

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
		Token elifKeyword = EatToken(TokenKind.ElifKeyword);
		Token openParen = EatToken(TokenKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		Token closeParen = EatToken(TokenKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		return new(elifKeyword, openParen, condition, closeParen, statement);
	}

	private ExpressionSyntax ParseExpression(Precedence precedence = default)
	{
		Token token = Peek();
		SyntaxKind kind = GetPrefixUnaryExpressionKind(token.Kind);

		if (kind != default)
		{
			EatToken();
			ExpressionSyntax expr = ParseExpression(GetPrecedence(kind));
			return new PrefixUnaryExpression(kind, token, expr)
			{
				Position = token.Position
			};
		}

		ExpressionSyntax primary = ParsePrimaryExpression();
		primary = ParsePostfixExpression(primary);

		ExpressionSyntax current = primary;

		while (TryParseSubExpression(current, precedence) is ExpressionSyntax expr)
		{
			current = expr;
		}

		if (PeekKind(TokenKind.QuestionToken) && precedence <= Precedence.Conditional)
		{
			Token questionToken = EatToken();
			ExpressionSyntax trueExpression = ParseExpression();
			Token colonToken = EatToken(TokenKind.ColonToken);
			ExpressionSyntax falseExpression = ParseExpression();

			current = new ConditionalExpressionSyntax(current, questionToken, trueExpression, colonToken, falseExpression)
			{
				Position = current.Position
			};
		}

		return current;
	}

	private ExpressionSyntax? TryParseSubExpression(ExpressionSyntax left, Precedence precedence)
	{
		SyntaxKind exprKind = GetBinaryExpressionKind(PeekKind());

		Precedence newPrecedence = GetPrecedence(exprKind);

		if (newPrecedence < precedence)
		{
			return null;
		}

		if (newPrecedence == precedence && !IsRightAssociative(exprKind))
		{
			return null;
		}

		Token operatorToken = EatToken();
		ExpressionSyntax right = ParseExpression(newPrecedence);

		if (SyntaxFacts.IsAssignmentOperator(operatorToken.Kind))
		{
			return new AssignmentExpressionSyntax(left, operatorToken, right)
			{
				Position = left.Position
			};
		}

		return new BinaryExpressionSyntax(exprKind, left, operatorToken, right)
		{
			Position = left.Position,
		};
	}

	private ExpressionSyntax ParsePrimaryExpression()
	{
		ref readonly Token token = ref Peek();

		switch (token.Kind)
		{
			case TokenKind.IdentifierToken:
				return ParseName();

			case TokenKind.SelfKeyword:
				EatToken();

				return new SelfExpressionSyntax(token)
				{
					Position = token.Position
				};

			case TokenKind.FalseKeyword:
			case TokenKind.TrueKeyword:
			case TokenKind.NumericLiteralToken:
			case TokenKind.StringLiteralToken:
			case TokenKind.CharLiteralToken:
				EatToken();

				return new LiteralExpressionSyntax(GetLiteralExpressionKind(token.Kind), token)
				{
					Position = token.Position
				};

			case TokenKind.OpenParenToken:

				if (TryParseCastExpression() is CastExpressionSyntax castExpr)
				{
					return castExpr;
				}

				Token openParen = EatToken();
				ExpressionSyntax expr = ParseExpression();
				Token closeParen = EatToken(TokenKind.CloseParenToken);

				return new ParenthesizedExpressionSyntax(openParen, expr, closeParen)
				{
					Position = openParen.Position,
				};

			default:
				if (SyntaxFacts.IsPredefinedType(token.Kind))
				{
					return new PredefinedTypeSyntax(token)
					{
						Position = token.Position
					};
				}

				return new IdentifierNameSyntax(MissingToken(token))
				{
					Position = token.Position
				};
		}
	}

	private ExpressionSyntax ParsePostfixExpression(ExpressionSyntax left)
	{
		ExpressionSyntax expr = left;

		while (true)
		{
			Token token = Peek();

			switch (token.Kind)
			{
				case TokenKind.OpenParenToken:
					expr = new InvocationExpressionSyntax(expr, ParseArgumentList())
					{
						Position = expr.Position,
					};

					break;

				case TokenKind.PlusPlusToken:
				case TokenKind.MinusMinusToken:
					expr = new PostfixUnaryExpression(GetPostfixUnaryExpressionKind(token.Kind), expr, EatToken())
					{
						Position = expr.Position,
					};

					break;

				case TokenKind.DotToken:
					expr = new MemberAccessExpressionSyntax(expr, EatToken(), ParseSimpleName())
					{
						Position = expr.Position,
					};

					break;

				default:
					return expr;
			}
		}
	}

	private ArgumentListSyntax ParseArgumentList()
	{
		Token openParen = EatToken(TokenKind.OpenParenToken);

		List<(ArgumentSyntax, Token)> args = new();

		while (true)
		{
			Token token = Peek();

			if (token.IsKind(TokenKind.CloseParenToken))
			{
				break;
			}

			ArgumentSyntax arg = ParseArgument();

			token = Peek();

			if (token.IsKind(TokenKind.CommaToken))
			{
				args.Add((arg, token));
				continue;
			}

			args.Add((arg, default));
		}

		Token closeParen = EatToken(TokenKind.CloseParenToken);

		return new(openParen, List(args), closeParen)
		{
			Position = openParen.Position
		};
	}

	private ArgumentSyntax ParseArgument()
	{
		ExpressionSyntax expr = ParseExpression();

		return new(expr);
	}

	private CastExpressionSyntax? TryParseCastExpression()
	{
		using Snapshot snapshot = TakeSnapshot();

		Token openParen = EatToken();

		if (openParen.Kind != TokenKind.OpenParenToken)
		{
			return null;
		}

		Token token = Peek();

		if (!token.IsPredefinedType())
		{
			return null;
		}

		TypeSyntax type = ParseType();

		Token closeParen = EatToken();

		if (closeParen.Kind != TokenKind.CloseParenToken)
		{
			return null;
		}

		snapshot.Accept();

		ExpressionSyntax expr = ParseExpression(Precedence.Cast);
		return new(openParen, type, closeParen, expr)
		{
			Position = openParen.Position,
		};
	}

	private TypeSyntax ParseType()
	{
		Token token = Peek();

		if (token.IsPredefinedType())
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

		while (PeekKind(TokenKind.DotToken))
		{
			Token dot = EatToken();

			IdentifierNameSyntax right = ParseIdentifierName();
			name = new QualifiedNameSyntax(name, dot, right)
			{
				Position = name.Position
			};
		}

		return name;
	}

	private SimpleNameSyntax ParseSimpleName()
	{
		Token identifier = EatToken(TokenKind.IdentifierToken);

		if (!PeekKind(TokenKind.LessThanToken))
		{
			return new IdentifierNameSyntax(identifier)
			{
				Position = identifier.Position
			};
		}

		Token lessThanToken = EatToken();
		List<(TypeSyntax, Token)> args = new();

		while (true)
		{
			TypeSyntax type = ParseType();

			if (PeekKind(TokenKind.CommaToken))
			{
				args.Add((type, EatToken()));
				continue;
			}

			if (PeekKind(TokenKind.GreaterThanToken))
			{
				args.Add((type, default));
				break;
			}
		}

		Token greaterThanToken = EatToken(TokenKind.GreaterThanToken);

		TypeArgumentListSyntax list = new(lessThanToken, List(args), greaterThanToken)
		{
			Position = lessThanToken.Position
		};

		return new GenericNameSyntax(identifier, list)
		{
			Position = identifier.Position
		};
	}

	private IdentifierNameSyntax ParseIdentifierName()
	{
		Token identifier = EatToken(TokenKind.IdentifierToken);

		return new(identifier)
		{
			Position = identifier.Position
		};
	}

	private static SyntaxKind GetPrefixUnaryExpressionKind(TokenKind kind)
	{
		return kind switch
		{
			TokenKind.PlusToken => SyntaxKind.UnaryPlusExpression,
			TokenKind.PlusPlusToken => SyntaxKind.PreIncrementExpression,
			TokenKind.MinusToken => SyntaxKind.UnaryMinusExpression,
			TokenKind.MinusMinusToken => SyntaxKind.PreDecrementExpression,
			TokenKind.ExclamationToken => SyntaxKind.LogicalNotExpression,
			TokenKind.TildeToken => SyntaxKind.BitwiseNotExpression,
			_ => default
		};
	}

	private static SyntaxKind GetPostfixUnaryExpressionKind(TokenKind kind)
	{
		return kind switch
		{
			TokenKind.PlusPlusToken => SyntaxKind.PostIncrementExpression,
			TokenKind.MinusMinusToken => SyntaxKind.PostDecrementExpression,
			_ => default
		};
	}

	private static SyntaxKind GetBinaryExpressionKind(TokenKind kind)
	{
		return kind switch
		{
			TokenKind.PlusToken => SyntaxKind.AddExpression,
			TokenKind.MinusToken => SyntaxKind.SubtractExpression,
			TokenKind.AsteriskToken => SyntaxKind.MultiplyExpression,
			TokenKind.SlashToken => SyntaxKind.DivideExpression,
			TokenKind.PercentToken => SyntaxKind.ModuloExpression,
			TokenKind.CaretToken => SyntaxKind.ExclusiveOrExpression,
			TokenKind.BarToken => SyntaxKind.BitwiseOrExpression,
			TokenKind.AmpersandToken => SyntaxKind.BitwiseAndExpression,
			TokenKind.GreaterThanGreaterThanToken => SyntaxKind.RightShiftExpression,
			TokenKind.LessThanLessThanToken => SyntaxKind.LeftShiftExpression,
			TokenKind.GreaterThanGreaterThanGreaterThanToken => SyntaxKind.UnsignedRightShiftExpression,
			TokenKind.EqualsEqualsToken => SyntaxKind.EqualsExpression,
			TokenKind.ExclamationEqualsToken => SyntaxKind.NotEqualsExpression,
			TokenKind.GreaterThanToken => SyntaxKind.GreaterThanExpression,
			TokenKind.GreaterThanEqualsToken => SyntaxKind.GreaterThanOrEqualExpression,
			TokenKind.LessThanToken => SyntaxKind.LessThanExpression,
			TokenKind.LessThanEqualsToken => SyntaxKind.LessThanOrEqualExpression,
			TokenKind.BarBarToken => SyntaxKind.LogicalOrExpression,
			TokenKind.AmpersandAmpersandToken => SyntaxKind.LogicalAndExpression,

			// Assignment

			TokenKind.EqualsToken => SyntaxKind.AssignmentExpression,
			TokenKind.PlusEqualsToken => SyntaxKind.AddAssignmentExpression,
			TokenKind.MinusEqualsToken => SyntaxKind.SubtractAssignmentExpression,
			TokenKind.AsteriskEqualsToken => SyntaxKind.MultiplyAssignmentExpression,
			TokenKind.SlashEqualsToken => SyntaxKind.DivideAssignmentExpression,
			TokenKind.PercentEqualsToken => SyntaxKind.ModuloAssignmentExpression,
			TokenKind.CaretEqualsToken => SyntaxKind.ExclusiveOrAssignmentExpression,
			TokenKind.BarEqualsToken => SyntaxKind.BitwiseOrExpression,
			TokenKind.AmpersandEqualsToken => SyntaxKind.BitwiseAndExpression,
			TokenKind.LessThanLessThanEqualsToken => SyntaxKind.LeftShiftAssignmentExpression,
			TokenKind.GreaterThanGreaterThanEqualsToken => SyntaxKind.RightShiftAssignmentExpression,
			TokenKind.GreaterThanGreaterThanGreaterThanEqualsToken => SyntaxKind.UnsignedRightShiftAssignmentExpression,
			_ => default
		};
	}

	private static SyntaxKind GetLiteralExpressionKind(TokenKind kind)
	{
		return kind switch
		{
			TokenKind.StringLiteralToken => SyntaxKind.StringLiteralExpression,
			TokenKind.NumericLiteralToken => SyntaxKind.NumericLiteralExpression,
			TokenKind.CharLiteralToken => SyntaxKind.CharLiteralExpression,
			TokenKind.TrueKeyword => SyntaxKind.TrueLiteralExpression,
			TokenKind.FalseKeyword => SyntaxKind.FalseLiteralExpression,
			_ => default
		};
	}

	private static Precedence GetPrecedence(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.CharLiteralExpression or
			SyntaxKind.FalseLiteralExpression or
			SyntaxKind.TrueLiteralExpression or
			SyntaxKind.StringLiteralExpression or
			SyntaxKind.NumericLiteralExpression or
			SyntaxKind.ParenthesizedExpression or
			SyntaxKind.IdentifierName or
			SyntaxKind.GenericName or
			SyntaxKind.PredefinedType or
			SyntaxKind.InvocationExpression or
			SyntaxKind.PostDecrementExpression or
			SyntaxKind.PostIncrementExpression or
			SyntaxKind.SelfExpression
				=> Precedence.Primary,

			SyntaxKind.CastExpression
				=> Precedence.Cast,

			SyntaxKind.UnaryPlusExpression or
			SyntaxKind.UnaryMinusExpression or
			SyntaxKind.BitwiseNotExpression or
			SyntaxKind.LogicalNotExpression or
			SyntaxKind.PreIncrementExpression or
			SyntaxKind.PreDecrementExpression
				=> Precedence.Unary,

			SyntaxKind.MultiplyExpression or
			SyntaxKind.DivideExpression or
			SyntaxKind.ModuloExpression
				=> Precedence.Multiplicative,

			SyntaxKind.AddExpression or
			SyntaxKind.SubtractExpression
				=> Precedence.Additive,

			SyntaxKind.LeftShiftExpression or
			SyntaxKind.RightShiftExpression or
			SyntaxKind.UnsignedRightShiftExpression
				=> Precedence.Shift,

			SyntaxKind.LessThanExpression or
			SyntaxKind.LessThanOrEqualExpression or
			SyntaxKind.GreaterThanExpression or
			SyntaxKind.GreaterThanOrEqualExpression
				=> Precedence.Relational,

			SyntaxKind.EqualsExpression or
			SyntaxKind.NotEqualsExpression
				=> Precedence.Equality,

			SyntaxKind.BitwiseAndExpression
				=> Precedence.BitwiseAnd,

			SyntaxKind.ExclusiveOrExpression
				=> Precedence.BitwiseXor,

			SyntaxKind.BitwiseOrExpression
				=> Precedence.BitwiseOr,

			SyntaxKind.LogicalAndExpression
				=> Precedence.ConditionalAnd,

			SyntaxKind.LogicalOrExpression
				=> Precedence.ConditionalOr,

			SyntaxKind.AssignmentExpression or
			SyntaxKind.AddAssignmentExpression or
			SyntaxKind.SubtractAssignmentExpression or
			SyntaxKind.MultiplyAssignmentExpression or
			SyntaxKind.DivideAssignmentExpression or
			SyntaxKind.ModuloAssignmentExpression or
			SyntaxKind.AndAssignmentExpression or
			SyntaxKind.ExclusiveOrAssignmentExpression or
			SyntaxKind.OrAssignmentExpression or
			SyntaxKind.LeftShiftAssignmentExpression or
			SyntaxKind.RightShiftAssignmentExpression or
			SyntaxKind.UnsignedRightShiftAssignmentExpression
				=> Precedence.Assignment,

			SyntaxKind.ConditionalExpression
				=> Precedence.Low,

			_ => default,
		};
	}

	private static bool IsRightAssociative(SyntaxKind kind)
	{
		return
			kind == SyntaxKind.AssignmentExpression ||
			kind == SyntaxKind.AddAssignmentExpression ||
			kind == SyntaxKind.SubtractAssignmentExpression ||
			kind == SyntaxKind.MultiplyAssignmentExpression ||
			kind == SyntaxKind.DivideAssignmentExpression ||
			kind == SyntaxKind.ModuloAssignmentExpression ||
			kind == SyntaxKind.AndAssignmentExpression ||
			kind == SyntaxKind.ExclusiveOrAssignmentExpression ||
			kind == SyntaxKind.OrAssignmentExpression ||
			kind == SyntaxKind.LeftShiftAssignmentExpression ||
			kind == SyntaxKind.RightShiftAssignmentExpression ||
			kind == SyntaxKind.UnsignedRightShiftAssignmentExpression;
	}

	private static SyntaxList<TNode> List<TNode>(List<TNode> nodes) where TNode : SyntaxNode
	{
		return new(nodes.ToArray());
	}

	private static SyntaxList<TNode> ListIfNotNull<TNode>(List<TNode>? nodes) where TNode : SyntaxNode
	{
		if (nodes is null)
		{
			return default;
		}

		return List(nodes);
	}

	private static SyntaxList<TNode> List<TNode>(TNode[] nodes) where TNode : SyntaxNode
	{
		return new(nodes);
	}

	private static TokenList List(List<Token> tokens)
	{
		return new(tokens.ToArray());
	}

	private static SeparatedSyntaxList<TNode> List<TNode>(List<(TNode node, Token seprator)> nodes) where TNode : SyntaxNode
	{
		return new(nodes.ToArray());
	}

	private Token EatToken(TokenKind kind)
	{
		ref readonly Token current = ref Peek();

		if (current.Kind != kind)
		{
			if (current.Kind == TokenKind.EOF)
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

	private ref readonly Token EatToken()
	{
		return ref _tokens[_current++];
	}

	private ref readonly Token Peek()
	{
		return ref _tokens[_current];
	}

	private ref readonly Token Peek(int pos)
	{
		int next = _current + pos;

		// Return EOF if pos is too big.
		if (next > _tokens.Length - 1)
		{
			return ref _tokens[^1];
		}

		return ref _tokens[_current + pos];
	}

	private Snapshot TakeSnapshot()
	{
		return new(this);
	}

	private TokenKind PeekKind()
	{
		ref readonly Token token = ref Peek();
		return token.Kind;
	}

	private bool PeekKind(TokenKind kind)
	{
		ref readonly Token token = ref Peek();
		return token.Kind == kind;
	}

	private static Token MissingToken(in Token current)
	{
		return new(TokenKind.MissingToken, string.Empty, current.Position);
	}

	private Token UnexpectedToken()
	{
		ref readonly Token current = ref EatToken();
		return UnexpectedToken(in current);
	}

	private static Token UnexpectedToken(in Token token)
	{
		return new(TokenKind.BadToken, token.Text, token.Position);
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

	private ref struct Snapshot : IDisposable
	{
		private readonly SourceParser _parser;
		private readonly int _pos;
		private bool _isAccepted;

		public Snapshot(SourceParser parser)
		{
			_parser = parser;
			_pos = parser._current;
		}

		public readonly void Reset()
		{
			_parser._current = _pos;
		}

		public void Accept()
		{
			_isAccepted = true;
		}

		public readonly void Dispose()
		{
			if (!_isAccepted)
			{
				Reset();
			}
		}
	}

	private enum Precedence : uint
	{
		Low = 0,

		// a = b, a += b, a >>= b etc.
		Assignment,

		// a ? b : c
		Conditional,

		//NullCoalescing,

		// a || b
		ConditionalOr,

		// a && b
		ConditionalAnd,

		// a | b
		BitwiseOr,

		// a ^ b
		BitwiseXor,

		// a & b
		BitwiseAnd,

		// a == b, a != b
		Equality,

		// a < b, a > b, etc.
		Relational,

		// a << b, a >>> b etc.
		Shift,

		// a + b, a - b
		Additive,

		// a * b, a / b
		Multiplicative,

		// match (a)
		//Match,

		// a..b
		//Range

		// ++a, -a, !a, true/false etc.
		Unary,

		// (int) a
		Cast,

		// a.b, a++, etc.
		Primary
	}
}
