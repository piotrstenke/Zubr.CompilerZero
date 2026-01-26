using System;
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

		while (!(token = Peek()).IsKind(SyntaxKind.EOF))
		{
			switch (token.Kind)
			{
				case SyntaxKind.UseKeyword:
					uses.Add(ParseUseDirective());
					break;

				case SyntaxKind.ModuleKeyword:
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

	private ModuleDeclarationSyntax ParseModuleDeclaration()
	{
		SyntaxToken moduleKeyword = EatToken();
		SyntaxToken topKeyword;

		NameSyntax? name;

		SyntaxToken semicolonToken;

		if (PeekKind(SyntaxKind.TopKeyword))
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

			if (token.IsPredefinedType())
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

		if (!lessThanToken.IsKind(SyntaxKind.LessThanToken))
		{
			return null;
		}

		EatToken();

		List<(TypeParameterSyntax, SyntaxToken)> parameters = new();

		while (true)
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

		if (!whereKeyword.IsKind(SyntaxKind.WhereKeyword))
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
				if (TryParseConstraint(Peek()) is not TypeParameterConstraintSyntax constraint)
				{
					break;
				}

				// This is the last constraint, so comma is not required.
				if (!Peek().IsKind(SyntaxKind.CommaToken))
				{
					constraints.Add((constraint, default));
					break;
				}

				commaToken = EatToken();

				// If the next token is an identifier, it could mean either:
				//
				// 1. Another type constraint for the current constraint clause.
				// 2. Identifier of type parameter in the next constraint clause.
				if (!Peek().IsKind(SyntaxKind.IdentifierToken))
				{
					// Not an identifier, so we can go directly to the next constraint.
					constraints.Add((constraint, commaToken));
					continue;
				}

				// Scenario 1, so add the constraint with comma and go to the next constraint.
				if (!Peek(1).IsKind(SyntaxKind.ColonToken))
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
			if (!Peek().IsKind(SyntaxKind.IdentifierToken))
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
		switch (token.Kind)
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

		if (PeekKind(SyntaxKind.MutKeyword))
		{
			modifiers.Add(EatToken());
		}

		TypeSyntax type = ParseType();

		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

		EqualsValueClauseSyntax? @default = null;

		if (PeekKind(SyntaxKind.EqualsToken))
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
			if (!modifier.IsAccessModifier())
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

		while (true)
		{
			SyntaxToken token = Peek();

			if (token.IsKind(SyntaxKind.EOF) || token.IsKind(SyntaxKind.CloseBraceToken))
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

		return token.Kind switch
		{
			SyntaxKind.OpenBraceToken => ParseBlock(),
			SyntaxKind.IfKeyword => ParseIfStatement(),
			SyntaxKind.WhileKeyword => ParseWhileStatement(),
			SyntaxKind.DoKeyword => ParseDoStatement(),
			SyntaxKind.ForKeyword => ParseForStatement(),
			SyntaxKind.IdentifierToken => ParseLocalOrExpressionStatement(),
			SyntaxKind.ReturnKeyword => ParseReturnStatement(),
			SyntaxKind.NextKeyword => ParseNextStatement(),
			SyntaxKind.StopKeyword => ParseStopStatement(),
			_ => ParseLocalOrExpressionStatement(),
		};
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

	private StatementSyntax ParseLocalOrExpressionStatement()
	{
		SyntaxToken token = Peek();

		if(token.IsPredefinedType() && Peek(1).Kind != SyntaxKind.DotToken)
		{
			return ParseLocalDeclaration();
		}

		ExpressionSyntax expr = ParseExpression();
		SyntaxToken semicolonToken = EatToken();

		return new ExpressionStatementSyntax(expr, semicolonToken)
		{
			Position = expr.Position,
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

		if (PeekKind(SyntaxKind.ElifKeyword))
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

		if (PeekKind(SyntaxKind.ElseKeyword))
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

	private ExpressionSyntax ParseExpression(Precedence precedence = default)
	{
		SyntaxToken token = Peek();
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

		while(TryParseSubExpression(current, precedence) is ExpressionSyntax expr)
		{
			current = expr;
		}

		if(PeekKind(SyntaxKind.QuestionToken) && precedence <= Precedence.Conditional)
		{
			SyntaxToken questionToken = EatToken();
			ExpressionSyntax trueExpression = ParseExpression();
			SyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
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

		SyntaxToken operatorToken = EatToken();
		ExpressionSyntax right = ParseExpression(newPrecedence);

		if(SyntaxFacts.IsAssignmentOperator(operatorToken.Kind))
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
		ref readonly SyntaxToken token = ref Peek();

		switch (token.Kind)
		{
			case SyntaxKind.IdentifierToken:
				return ParseName();

			case SyntaxKind.SelfKeyword:
				EatToken();

				return new SelfExpressionSyntax(token)
				{
					Position = token.Position
				};

			case SyntaxKind.FalseKeyword:
			case SyntaxKind.TrueKeyword:
			case SyntaxKind.NumericLiteralToken:
			case SyntaxKind.StringLiteralToken:
			case SyntaxKind.CharLiteralToken:
				EatToken();

				return new LiteralExpressionSyntax(GetLiteralExpressionKind(token.Kind), token)
				{
					Position = token.Position
				};

			case SyntaxKind.OpenParenToken:

				if (TryParseCastExpression() is CastExpressionSyntax castExpr)
				{
					return castExpr;
				}

				SyntaxToken openParen = EatToken();
				ExpressionSyntax expr = ParseExpression();
				SyntaxToken closeParen = EatToken(SyntaxKind.CloseParenToken);

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
			SyntaxToken token = Peek();

			switch (token.Kind)
			{
				case SyntaxKind.OpenParenToken:
					expr = new InvocationExpressionSyntax(expr, ParseArgumentList())
					{
						Position = expr.Position,
					};

					break;

				case SyntaxKind.PlusPlusToken:
				case SyntaxKind.MinusMinusToken:
					expr = new PostfixUnaryExpression(GetPostfixUnaryExpressionKind(token.Kind), expr, EatToken())
					{
						Position = expr.Position,
					};

					break;

				case SyntaxKind.DotToken:
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
		SyntaxToken openParen = EatToken(SyntaxKind.OpenParenToken);

		List<(ArgumentSyntax, SyntaxToken)> args = new();

		while(true)
		{
			SyntaxToken token = Peek();

			if(token.IsKind(SyntaxKind.CloseParenToken))
			{
				break;
			}

			ArgumentSyntax arg = ParseArgument();

			token = Peek();

			if(token.IsKind(SyntaxKind.CommaToken))
			{
				args.Add((arg, token));
				continue;
			}

			args.Add((arg, default));
		}

		SyntaxToken closeParen = EatToken(SyntaxKind.CloseParenToken);

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

		SyntaxToken openParen = EatToken();

		if (openParen.Kind != SyntaxKind.OpenParenToken)
		{
			return null;
		}

		SyntaxToken token = Peek();

		if(!token.IsPredefinedType())
		{
			return null;
		}

		TypeSyntax type = ParseType();

		SyntaxToken closeParen = EatToken();

		if(closeParen.Kind != SyntaxKind.CloseParenToken)
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
		SyntaxToken token = Peek();

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

		while (PeekKind(SyntaxKind.DotToken))
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

	private SimpleNameSyntax ParseSimpleName()
	{
		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

		if(!PeekKind(SyntaxKind.LessThanToken))
		{
			return new IdentifierNameSyntax(identifier)
			{
				Position = identifier.Position
			};
		}

		SyntaxToken lessThanToken = EatToken();
		List<(TypeSyntax, SyntaxToken)> args = new();

		while (true)
		{
			TypeSyntax type = ParseType();

			if(PeekKind(SyntaxKind.CommaToken))
			{
				args.Add((type, EatToken()));
				continue;
			}

			if(PeekKind(SyntaxKind.GreaterThanToken))
			{
				args.Add((type, default));
				break;
			}
		}

		SyntaxToken greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);

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
		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);

		return new(identifier)
		{
			Position = identifier.Position
		};
	}

	private static SyntaxKind GetPrefixUnaryExpressionKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PlusToken => SyntaxKind.UnaryPlusExpression,
			SyntaxKind.PlusPlusToken => SyntaxKind.PreIncrementExpression,
			SyntaxKind.MinusToken => SyntaxKind.UnaryMinusExpression,
			SyntaxKind.MinusMinusToken => SyntaxKind.PreDecrementExpression,
			SyntaxKind.ExclamationToken => SyntaxKind.LogicalNotExpression,
			SyntaxKind.TildeToken => SyntaxKind.BitwiseNotExpression,
			_ => default
		};
	}

	private static SyntaxKind GetPostfixUnaryExpressionKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PlusPlusToken => SyntaxKind.PostIncrementExpression,
			SyntaxKind.MinusMinusToken => SyntaxKind.PostDecrementExpression,
			_ => default
		};
	}

	private static SyntaxKind GetBinaryExpressionKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PlusToken => SyntaxKind.AddExpression,
			SyntaxKind.MinusToken => SyntaxKind.SubtractExpression,
			SyntaxKind.AsteriskToken => SyntaxKind.MultiplyExpression,
			SyntaxKind.SlashToken => SyntaxKind.DivideExpression,
			SyntaxKind.PercentToken => SyntaxKind.ModuloExpression,
			SyntaxKind.CaretToken => SyntaxKind.ExclusiveOrExpression,
			SyntaxKind.BarToken => SyntaxKind.BitwiseOrExpression,
			SyntaxKind.AmpersandToken => SyntaxKind.BitwiseAndExpression,
			SyntaxKind.GreaterThanGreaterThanToken => SyntaxKind.RightShiftExpression,
			SyntaxKind.LessThanLessThanToken => SyntaxKind.LeftShiftExpression,
			SyntaxKind.GreaterThanGreaterThanGreaterThanToken => SyntaxKind.UnsignedRightShiftExpression,
			SyntaxKind.EqualsEqualsToken => SyntaxKind.EqualsExpression,
			SyntaxKind.ExclamationEqualsToken => SyntaxKind.NotEqualsExpression,
			SyntaxKind.GreaterThanToken => SyntaxKind.GreaterThanExpression,
			SyntaxKind.GreaterThanEqualsToken => SyntaxKind.GreaterThanOrEqualExpression,
			SyntaxKind.LessThanToken => SyntaxKind.LessThanExpression,
			SyntaxKind.LessThanEqualsToken => SyntaxKind.LessThanOrEqualExpression,
			SyntaxKind.BarBarToken => SyntaxKind.LogicalOrExpression,
			SyntaxKind.AmpersandAmpersandToken => SyntaxKind.LogicalAndExpression,

			// Assignment

			SyntaxKind.EqualsToken => SyntaxKind.AssignmentExpression,
			SyntaxKind.PlusEqualsToken => SyntaxKind.AddAssignmentExpression,
			SyntaxKind.MinusEqualsToken => SyntaxKind.SubtractAssignmentExpression,
			SyntaxKind.AsteriskEqualsToken => SyntaxKind.MultiplyAssignmentExpression,
			SyntaxKind.SlashEqualsToken => SyntaxKind.DivideAssignmentExpression,
			SyntaxKind.PercentEqualsToken => SyntaxKind.ModuloAssignmentExpression,
			SyntaxKind.CaretEqualsToken => SyntaxKind.ExclusiveOrAssignmentExpression,
			SyntaxKind.BarEqualsToken => SyntaxKind.BitwiseOrExpression,
			SyntaxKind.AmpersandEqualsToken => SyntaxKind.BitwiseAndExpression,
			SyntaxKind.LessThanLessThanEqualsToken => SyntaxKind.LeftShiftAssignmentExpression,
			SyntaxKind.GreaterThanGreaterThanEqualsToken => SyntaxKind.RightShiftAssignmentExpression,
			SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken => SyntaxKind.UnsignedRightShiftAssignmentExpression,
			_ => default
		};
	}

	private static SyntaxKind GetLiteralExpressionKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.StringLiteralToken => SyntaxKind.StringLiteralExpression,
			SyntaxKind.NumericLiteralToken => SyntaxKind.NumericLiteralExpression,
			SyntaxKind.CharLiteralToken => SyntaxKind.CharLiteralExpression,
			SyntaxKind.TrueKeyword => SyntaxKind.TrueLiteralExpression,
			SyntaxKind.FalseKeyword => SyntaxKind.FalseLiteralExpression,
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

		if (current.Kind != kind)
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

	private SyntaxKind PeekKind()
	{
		ref readonly SyntaxToken token = ref Peek();
		return token.Kind;
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
