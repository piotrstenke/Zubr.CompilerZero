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
		List<AliasDirectiveSyntax> aliases = new();

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

				case TokenKind.AliasKeyword:
					aliases.Add(ParseAliasDirective());
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

		return new(List(uses), List(aliases), List(members), token)
		{
			Position = 0
		};
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
		Token useKeyword = EatToken(TokenKind.UseKeyword);

		NameSyntax name = ParseName();

		Token asKeyword = default;
		IdentifierNameSyntax? alias = null;

		Token fromKeyword = default;
		NameSyntax? moduleName = null;

		if (PeekKind(TokenKind.AsKeyword))
		{
			asKeyword = EatToken();
			alias = ParseIdentifierName();
		}

		if(PeekKind(TokenKind.FromKeyword))
		{
			fromKeyword = EatToken();
			moduleName = ParseName();
		}

		Token semicolon = EatToken(TokenKind.SemicolonToken);

		return new(useKeyword, name, asKeyword, alias, fromKeyword, moduleName, semicolon)
		{
			Position = useKeyword.Position
		};
	}

	private AliasDirectiveSyntax ParseAliasDirective()
	{
		TokenList modifiers = ParseModifiers();

		Token keyword = EatToken(TokenKind.AliasKeyword);
		SimpleNameSyntax alias = ParseSimpleName();

		Token equalsToken = EatToken(TokenKind.EqualsToken);

		NameSyntax name = ParseName();

		Token semicolonToken = EatToken(TokenKind.SemicolonToken);

		int position = modifiers.IsDefaultOrEmpty
			? keyword.Position
			: modifiers.GetPosition();

		return new(modifiers, keyword, alias, equalsToken, name, semicolonToken)
		{
			Position = position
		};
	}

	private MemberDeclarationSyntax? TryParseMemberDeclaration()
	{
		SyntaxList<AttributeSyntax> attributes = ParseAttributes();
		TokenList modifiers = ParseModifiers();

		Token token = Peek();
		int position = GetMemberPosition(attributes, modifiers, token);

		while (true)
		{
			token = Peek();

			if (token.IsPredefinedType())
			{
				return ParseFunctionOrPropertyDeclaration(attributes, modifiers, position);
			}

			return token.Kind switch
			{
				TokenKind.ClassKeyword
					=> ParseClassDeclaration(attributes, modifiers, position),

				TokenKind.StructKeyword
					=> ParseStructDeclaration(attributes, modifiers, position),

				TokenKind.EnumKeyword
					=> ParseEnumDeclaration(attributes, modifiers, position),

				TokenKind.TraitKeyword
					=> ParseTraitDeclaration(attributes, modifiers, position),

				TokenKind.AttrKeyword
					=> ParseAttributeDeclaration(attributes, modifiers, position),

				TokenKind.FieldKeyword
					=> ParseFieldDeclaration(attributes, modifiers, position),

				TokenKind.NewKeyword
					=> ParseConstructorDeclaration(attributes, modifiers, position),

				TokenKind.ImplKeyword
					=> ParseImplementationDeclaration(attributes, modifiers, position),

				TokenKind.FreeKeyword or
				TokenKind.GCFreeKeyword
					=> ParseDestructorDeclaration(attributes, modifiers, position),

				TokenKind.IdentifierToken
					=> ParseFunctionOrPropertyDeclaration(attributes, modifiers, position),

				_ => null,
			};
		}
	}

	private FieldDeclarationSyntax ParseFieldDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		Token fieldKeyword = EatToken(TokenKind.FieldKeyword);

		VariableDeclarationSyntax variable = ParseVariable();

		Token semicolonToken = EatToken(TokenKind.SemicolonToken);

		return new(attributes, modifiers, fieldKeyword, variable, semicolonToken)
		{
			Position = position
		};
	}

	private ConstructorDeclarationSyntax ParseConstructorDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		Token keyword = EatToken(TokenKind.NewKeyword);

		ParameterListSyntax parameterList = ParseParameterList();

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody();

		return new(attributes, modifiers, keyword, parameterList, body, expressionBody, semicolonToken)
		{
			Position = position
		};
	}

	private DestructorDeclarationSyntax ParseDestructorDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		Token keyword = EatToken();

		ParameterListSyntax parameterList = ParseParameterList();

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody();

		return new(attributes, modifiers, keyword, parameterList, body, expressionBody, semicolonToken)
		{
			Position = position
		};
	}

	private MemberDeclarationSyntax ParseFunctionOrPropertyDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		if(PeekKind(TokenKind.CastKeyword))
		{
			return ParseCastDeclaration(attributes, modifiers, position);
		}

		TypeSyntax returnType = ParseType();

		Token token = EatToken();

		switch(token.Kind)
		{
			case TokenKind.SelfKeyword:
				if (PeekKind(TokenKind.OpenBracketToken))
				{
					return ParseIndexerDeclaration(attributes, modifiers, returnType, token, position);
				}

				return ParseInvokerDeclaration(attributes, modifiers, returnType, token, position);

			case TokenKind.OperKeyword:
				return ParseOperatorDeclaration(attributes, modifiers, returnType, token, position);

			default:
				EnsureKind(ref token, TokenKind.IdentifierToken);

				TypeParameterListSyntax? typeParameterList = TryParseTypeParameterList();

				if (typeParameterList is not null)
				{
					return ParseFunctionDeclaration(attributes, modifiers, returnType, token, typeParameterList, position);
				}

				ParameterListSyntax? parameterList = TryParseParameterList();

				if (parameterList is not null)
				{
					return ParseFunctionDeclaration(attributes, modifiers, returnType, token, typeParameterList, parameterList, position);
				}

				return ParsePropertyDeclaration(attributes, modifiers, returnType, token, position);
		}
	}

	private PropertyDeclarationSyntax ParsePropertyDeclaration(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax returnType,
		Token identifier,
		int position
	)
	{
		Token token = Peek();

		ArrowExpressionClauseSyntax? expressionBody;
		AccessorListSyntax? accessorList;
		EqualsValueClauseSyntax? initializer;
		Token semicolonToken;

		if (token.IsKind(TokenKind.EqualsGreaterThanToken))
		{
			EatToken();

			expressionBody = new(token, ParseExpression())
			{
				Position = token.Position
			};

			semicolonToken = EatToken(TokenKind.SemicolonToken);

			initializer = null;
			accessorList = null;
		}
		else
		{
			accessorList = TryParseAccesorList();
			initializer = TryParseEqualsValueClause();
			expressionBody = null;

			if (accessorList is null)
			{
				semicolonToken = EatToken(TokenKind.SemicolonToken);
			}
			else
			{
				semicolonToken = default;
			}
		}

		return new(attributes, modifiers, returnType, identifier, expressionBody, accessorList, initializer, semicolonToken)
		{
			Position = position
		};
	}

	private IndexerDeclarationSyntax ParseIndexerDeclaration(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax returnType,
		Token selfKeyword,
		int position
	)
	{
		BracketParameterListSyntax parameterList = ParseBracketParameterList();

		Token token = Peek();

		ArrowExpressionClauseSyntax? expressionBody;
		AccessorListSyntax? accessorList;
		Token semicolonToken;

		if (token.IsKind(TokenKind.EqualsGreaterThanToken))
		{
			EatToken();

			expressionBody = new(token, ParseExpression())
			{
				Position = token.Position
			};

			semicolonToken = EatToken(TokenKind.SemicolonToken);

			accessorList = null;
		}
		else
		{
			accessorList = TryParseAccesorList();
			expressionBody = null;

			if (accessorList is null)
			{
				semicolonToken = EatToken(TokenKind.SemicolonToken);
			}
			else
			{
				semicolonToken = default;
			}
		}

		return new(attributes, modifiers, returnType, selfKeyword, parameterList, expressionBody, accessorList, semicolonToken)
		{
			Position = position
		};
	}

	private AccessorListSyntax? TryParseAccesorList()
	{
		if(!PeekKind(TokenKind.OpenBraceToken))
		{
			return null;
		}

		Token openBrace = EatToken();

		List<AccessorDeclarationSyntax> accessors = new();

		Token token;

		while((token = Peek()).IsValid())
		{
			if(token.IsKind(TokenKind.CloseBraceToken))
			{
				break;
			}

			SyntaxList<AttributeSyntax> attributes = ParseAttributes();
			TokenList modifiers = ParseModifiers();

			Token keyword = EatToken();

			int position = GetMemberPosition(attributes, modifiers, keyword);

			SyntaxKind kind = keyword.GetAccessorKind();

			if(kind == default)
			{
				AddError(ErrorCode.ERR_SyntaxError);
				keyword = UnexpectedToken();
			}

			(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody();

			accessors.Add(new(kind, attributes, modifiers, keyword, body, expressionBody, semicolonToken)
			{
				Position = position
			});
		}

		Token closeBrace = EatToken(TokenKind.CloseBraceToken);

		return new(openBrace, List(accessors), closeBrace)
		{
			Position = openBrace.Position,
		};
	}

	private CastDeclarationSyntax ParseCastDeclaration(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		int position
	)
	{
		Token keyword = EatToken(TokenKind.CastKeyword);

		TypeSyntax type = ParseType();

		ParameterListSyntax parameterList = ParseParameterList();

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody();

		return new(attributes, modifiers, keyword, type, parameterList, body, expressionBody, semicolonToken)
		{
			Position = position
		};
	}

	private OperatorDeclarationSyntax ParseOperatorDeclaration(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax returnType,
		Token keyword,
		int position
	)
	{
		Token token = EatToken();

		if(!token.IsOverloadableOperator())
		{
			AddError(ErrorCode.ERR_SyntaxError, token.Position);
			token = UnexpectedToken(token);
		}

		ParameterListSyntax parameterList = ParseParameterList();

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody();

		return new(attributes, modifiers, returnType, keyword, token, parameterList, body, expressionBody, semicolonToken)
		{
			Position = position
		};
	}

	private InvokerDeclarationSyntax ParseInvokerDeclaration(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax returnType,
		Token selfKeyword,
		int position
	)
	{
		ParameterListSyntax parameterList = ParseParameterList();

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody();

		return new(attributes, modifiers, returnType, selfKeyword, parameterList, body, expressionBody, semicolonToken)
		{
			Position = position
		};
	}

	private FunctionDeclarationSyntax ParseFunctionDeclaration(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax returnType,
		Token identifier,
		TypeParameterListSyntax typeParameterList,
		int position
	)
	{
		ParameterListSyntax parameterList = ParseParameterList();
		return ParseFunctionDeclaration(attributes, modifiers, returnType, identifier, typeParameterList, parameterList, position);
	}

	private FunctionDeclarationSyntax ParseFunctionDeclaration(
		SyntaxList<AttributeSyntax> attributes,
		TokenList modifiers,
		TypeSyntax returnType,
		Token identifier,
		TypeParameterListSyntax? typeParameterList,
		ParameterListSyntax parameterList,
		int position
	)
	{
		TypeParameterConstraintListSyntax? constraintList = TryParseConstraintList();

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody();

		return new(attributes, modifiers, returnType, identifier, typeParameterList, parameterList, constraintList, body, expressionBody, semicolonToken)
		{
			Position = position
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

	private BracketParameterListSyntax ParseBracketParameterList()
	{
		Token openBracket = EatToken(TokenKind.OpenBracketToken);

		List<(ParameterSyntax parameter, Token separator)> parameters = new();

		Token token = Peek();

		if (token.IsKind(TokenKind.CloseBracketToken))
		{
			EatToken();
		}
		else
		{
			while (token.IsValid())
			{
				ParameterSyntax parameter = ParseParameter();

				token = Peek();

				if (token.IsKind(TokenKind.CloseBracketToken))
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

		return new(openBracket, List(parameters), token)
		{
			Position = openBracket.Position
		};
	}

	private ParameterListSyntax? TryParseParameterList()
	{
		if (!PeekKind(TokenKind.OpenParenToken))
		{
			return null;
		}

		return ParseParameterList();
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
		SyntaxList<AttributeSyntax> attributes = ParseAttributes();
		TokenList modifiers = ParseModifiers();

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

		return new(attributes, modifiers, type, identifier, @default)
		{
			Position = identifier.Position
		};
	}

	private TokenList ParseModifiers()
	{
		Token token = Peek();

		if(!token.IsModifier())
		{
			return TokenList.Empty;
		}

		EatToken();

		List<Token> tokens = new()
		{
			token
		};

		while((token = Peek()).IsModifier())
		{
			EatToken();
			tokens.Add(token);
		}

		return List(tokens);
	}

	private ImplementationDeclarationSyntax ParseImplementationDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		Token keyword = EatToken(TokenKind.ImplKeyword);

		TypeParameterListSyntax? typeParameterList = TryParseTypeParameterList();

		TypeSyntax type = ParseType();

		BaseTypeListSyntax? baseTypeList = TryParseBaseTypeList();

		TypeParameterConstraintListSyntax? constraints = TryParseConstraintList();

		Token openBrace;
		SyntaxList<MemberDeclarationSyntax> members;
		Token closeBrace;

		Token semicolonToken;

		if (PeekKind(TokenKind.SemicolonToken))
		{
			semicolonToken = EatToken();
			openBrace = default;
			members = default;
			closeBrace = default;
		}
		else
		{
			openBrace = EatToken(TokenKind.OpenBraceToken);
			members = ParseTypeMembers();
			closeBrace = EatToken(TokenKind.CloseBraceToken);
			semicolonToken = default;
		}

		return new(attributes, modifiers, keyword, typeParameterList, type, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace)
		{
			Position = position
		};
	}

	private BaseEnumDeclarationSyntax ParseEnumDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		Token keyword = EatToken(TokenKind.EnumKeyword);

		Token nextKeyword = Peek();

		if(nextKeyword.IsKind(TokenKind.ClassKeyword) || nextKeyword.IsKind(TokenKind.StructKeyword))
		{
			EatToken();
		}

		Token identifier = EatToken(TokenKind.IdentifierToken);
		ParameterListSyntax? parameterList = TryParseParameterList();
		BaseTypeListSyntax? baseTypeList = TryParseBaseTypeList();

		Token openBrace;
		SeparatedSyntaxList<EnumMemberDeclarationSyntax> members;
		Token closeBrace;

		Token semicolonToken;

		if (PeekKind(TokenKind.SemicolonToken))
		{
			semicolonToken = EatToken();
			openBrace = default;
			members = default;
			closeBrace = default;
		}
		else
		{
			openBrace = EatToken(TokenKind.OpenBraceToken);
			members = ParseEnumMembers();
			closeBrace = EatToken(TokenKind.CloseBraceToken);
			semicolonToken = default;
		}

		if (nextKeyword.IsKind(TokenKind.ClassKeyword))
		{
			return new EnumClassDeclarationSyntax(attributes, modifiers, keyword, nextKeyword, identifier, parameterList, baseTypeList, semicolonToken, openBrace, members, closeBrace)
			{
				Position = position
			};
		}

		if(nextKeyword.IsKind(TokenKind.StructKeyword))
		{
			return new EnumStructDeclarationSyntax(attributes, modifiers, keyword, nextKeyword, identifier, parameterList, baseTypeList, semicolonToken, openBrace, members, closeBrace)
			{
				Position = position
			};
		}

		return new SimpleEnumDeclarationSyntax(attributes, modifiers, keyword, identifier, semicolonToken, openBrace, members, closeBrace)
		{
			Position = position
		};
	}

	private SeparatedSyntaxList<EnumMemberDeclarationSyntax> ParseEnumMembers()
	{
		List<(EnumMemberDeclarationSyntax, Token)> members = new();

		Token token;

		while ((token = Peek()).IsValid())
		{
			if(token.IsKind(TokenKind.CloseBraceToken))
			{
				break;
			}

			EnumMemberDeclarationSyntax member = ParseEnumMemberDeclaration();

			token = Peek();

			if(token.IsKind(TokenKind.CommaToken))
			{
				EatToken();
				members.Add((member, token));
				continue;
			}

			members.Add((member, default));
		}

		return List(members);
	}

	private EnumMemberDeclarationSyntax ParseEnumMemberDeclaration()
	{
		SyntaxList<AttributeSyntax> attributes = ParseAttributes();
		TokenList modifiers = ParseModifiers();

		Token identifier = EatToken(TokenKind.IdentifierToken);

		int position = GetMemberPosition(attributes, modifiers, identifier);

		ArgumentListSyntax? argumentList = TryParseArgumentList();

		if(argumentList is not null)
		{
			return new ComplexEnumMemberDeclarationSyntax(attributes, modifiers, identifier, argumentList)
			{
				Position = position
			};
		}

		EqualsValueClauseSyntax? initializer = TryParseEqualsValueClause();

		return new SimpleEnumMemberDeclarationSyntax(attributes, modifiers, identifier, initializer)
		{
			Position = position
		};
	}

	private AttributeDeclarationSyntax ParseAttributeDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		return (AttributeDeclarationSyntax)ParseTypeDeclaration(SyntaxKind.AttributeDeclaration, attributes, modifiers, position);
	}

	private TraitDeclarationSyntax ParseTraitDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		return (TraitDeclarationSyntax)ParseTypeDeclaration(SyntaxKind.TraitDeclaration, attributes, modifiers, position);
	}

	private ClassDeclarationSyntax ParseClassDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		return (ClassDeclarationSyntax)ParseTypeDeclaration(SyntaxKind.ClassDeclaration, attributes, modifiers, position);
	}

	private StructDeclarationSyntax ParseStructDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		foreach (Token modifier in modifiers)
		{
			if (!modifier.IsAccessModifier())
			{
				AddError(ErrorCode.ERR_InvalidModifier, modifier.Position);
			}
		}

		return (StructDeclarationSyntax)ParseTypeDeclaration(SyntaxKind.StructDeclaration, attributes, modifiers, position);
	}

	private TypeDeclarationSyntax ParseTypeDeclaration(SyntaxKind kind, SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		Token keyword = EatToken();

		Token identifier = EatToken(TokenKind.IdentifierToken);

		TypeParameterListSyntax? typeParameterList = TryParseTypeParameterList();

		ParameterListSyntax? parameterList = TryParseParameterList();

		BaseTypeListSyntax? baseTypeList = TryParseBaseTypeList();

		TypeParameterConstraintListSyntax? constraints = TryParseConstraintList();

		Token openBrace;
		SyntaxList<MemberDeclarationSyntax> members;
		Token closeBrace;

		Token semicolonToken;

		if (PeekKind(TokenKind.SemicolonToken))
		{
			semicolonToken = EatToken();
			openBrace = default;
			members = default;
			closeBrace = default;
		}
		else
		{
			openBrace = EatToken(TokenKind.OpenBraceToken);
			members = ParseTypeMembers();
			closeBrace = EatToken(TokenKind.CloseBraceToken);
			semicolonToken = default;
		}

		return kind switch
		{
			SyntaxKind.ClassDeclaration => new ClassDeclarationSyntax(attributes, modifiers, keyword, identifier, typeParameterList, parameterList, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace)
			{
				Position = position
			},

			SyntaxKind.StructDeclaration => new StructDeclarationSyntax(attributes, modifiers, keyword, identifier, typeParameterList, parameterList, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace)
			{
				Position = position
			},

			SyntaxKind.TraitDeclaration => new TraitDeclarationSyntax(attributes, modifiers, keyword, identifier, typeParameterList, parameterList, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace)
			{
				Position = position
			},

			SyntaxKind.AttributeDeclaration => new AttributeDeclarationSyntax(attributes, modifiers, keyword, identifier, typeParameterList, parameterList, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace)
			{
				Position = position
			},

			_ => throw new UnreachableException()
		};
	}

	private SyntaxList<MemberDeclarationSyntax> ParseTypeMembers()
	{
		List<MemberDeclarationSyntax> members = new();

		Token token;

		while ((token = Peek()).IsValid() && !token.IsKind(TokenKind.CloseBraceToken))
		{
			if (TryParseMemberDeclaration() is MemberDeclarationSyntax member)
			{
				members.Add(member);
			}
		}

		return List(members);
	}

	private BaseTypeListSyntax? TryParseBaseTypeList()
	{
		ref readonly Token colonToken = ref Peek();

		if(!colonToken.IsKind(TokenKind.ColonToken))
		{
			return null;
		}

		EatToken();
		List<(BaseTypeSyntax, Token)> baseTypes = new();

		Token token;

		while((token = Peek()).IsValid())
		{
			if(ShouldStop(token))
			{
				break;
			}

			BaseTypeSyntax type = ParseBaseType();

			token = Peek();

			if(token.IsKind(TokenKind.CommaToken))
			{
				EatToken();
				baseTypes.Add((type, token));
				continue;
			}

			baseTypes.Add((type, default));
		}

		return new(colonToken, List(baseTypes))
		{
			Position = colonToken.Position,
		};

		static bool ShouldStop(Token token)
		{
			return token.Kind is
				TokenKind.WhereKeyword or
				TokenKind.OpenBraceToken or
				TokenKind.SemicolonToken;
		}
	}

	private BaseTypeSyntax ParseBaseType()
	{
		TypeSyntax type = ParseType();

		ArgumentListSyntax? argumentList = TryParseArgumentList();

		if(argumentList is null)
		{
			return new SimpleBaseTypeSyntax(type)
			{
				Position = type.Position
			};
		}

		return new PrimaryBaseTypeSyntax(type, argumentList)
		{
			Position = type.Position
		};
	}

	private (BlockSyntax? block, ArrowExpressionClauseSyntax? expression, Token semicolonToken) ParseBody()
	{
		ref readonly Token token = ref Peek();

		switch(token.Kind)
		{
			case TokenKind.OpenBraceToken:
				return (ParseBlock(), null, default);

			case TokenKind.EqualsGreaterThanToken:
				EatToken();
				ArrowExpressionClauseSyntax exprBody = new(token, ParseExpression())
				{
					Position = token.Position
				};

				Token semicolonToken = EatToken(TokenKind.SemicolonToken);
				return (null, exprBody, semicolonToken);

			case TokenKind.SemicolonToken:
				EatToken();
				return (null, null, token);

			default:
				return default;
		}
	}

	private BlockSyntax ParseBlock()
	{
		Token openBrace = EatToken(TokenKind.OpenBraceToken);

		List<StatementSyntax> statements = new();

		Token token;

		while ((token = Peek()).IsValid())
		{
			if (token.IsKind(TokenKind.CloseBraceToken))
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
		TokenList modifiers = ParseModifiers();

		VariableDeclarationSyntax variable = ParseVariable();

		int position = modifiers.IsDefaultOrEmpty
			? variable.Position
			: modifiers.GetPosition();

		Token semicolon = EatToken(TokenKind.SemicolonToken);

		if (variable.Type is PredefinedTypeSyntax p && p.Keyword.IsKind(TokenKind.VoidKeyword))
		{
			AddError(ErrorCode.ERR_SyntaxError);
		}

		return new(modifiers, variable, semicolon)
		{
			Position = position
		};
	}

	private VariableDeclarationSyntax ParseVariable()
	{
		TypeSyntax type = ParseType();

		Token identifier = EatToken(TokenKind.IdentifierToken);

		EqualsValueClauseSyntax? initializer = TryParseEqualsValueClause();

		return new(type, identifier, initializer)
		{
			Position = type.Position
		};
	}

	private EqualsValueClauseSyntax? TryParseEqualsValueClause()
	{
		Token equalsToken = Peek();

		if (!equalsToken.IsKind(TokenKind.EqualsToken))
		{
			return null;
		}

		EatToken();

		return new(equalsToken, ParseExpression())
		{
			Position = equalsToken.Position
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

	private SyntaxList<AttributeSyntax> ParseAttributes()
	{
		Token token = Peek();

		if(!token.IsKind(TokenKind.OpenBracketToken))
		{
			return default;
		}

		List<AttributeSyntax> attributes = new();

		while(TryParseAttribute() is AttributeSyntax attr)
		{
			attributes.Add(attr);
		}

		return List(attributes);
	}

	private AttributeSyntax? TryParseAttribute()
	{
		Token openBracket = Peek();

		if(!openBracket.IsKind(TokenKind.OpenBracketToken))
		{
			return null;
		}

		EatToken();
		AttributeTargetSyntax? target = TryParseAttributeTarget();

		NameSyntax name = ParseName();

		AttributeArgumentListSyntax? argumentList = TryParseAttributeArgumentList();

		Token closeBracket = EatToken(TokenKind.CloseBracketToken);

		return new(openBracket, target, name, argumentList, closeBracket)
		{
			Position = openBracket.Position
		};
	}

	private AttributeTargetSyntax? TryParseAttributeTarget()
	{
		Token token = Peek();

		// TODO: Handle all attribute target specifiers.
		switch (token.Kind)
		{
			case TokenKind.ReturnKeyword:
			case TokenKind.AssemblyKeyword:
			case TokenKind.FieldKeyword:
				Token targetKeyword = EatToken();
				Token colonToken = EatToken(TokenKind.ColonToken);

				return new(targetKeyword, colonToken)
				{
					Position = targetKeyword.Position,
				};

			default:
				return null;
		}
	}

	private AttributeArgumentListSyntax? TryParseAttributeArgumentList()
	{
		Token openParen = Peek();

		if(!openParen.IsKind(TokenKind.OpenParenToken))
		{
			return null;
		}

		List<(AttributeArgumentSyntax, Token)> args = new();

		while (true)
		{
			Token token = Peek();

			if (!token.IsValid() || token.IsKind(TokenKind.CloseParenToken))
			{
				break;
			}

			AttributeArgumentSyntax arg = ParseAttributeArgument();

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

	private AttributeArgumentSyntax ParseAttributeArgument()
	{
		ExpressionSyntax expr = ParseExpression();

		return new(expr);
	}

	private ExpressionSyntax ParseExpression(Precedence precedence = default)
	{
		Token token = Peek();
		SyntaxKind kind = SyntaxFacts.GetPrefixUnaryExpressionKind(token.Kind);

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
		SyntaxKind exprKind = SyntaxFacts.GetBinaryExpressionKind(PeekKind());

		if(exprKind == SyntaxKind.None)
		{
			return null;
		}

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

		if(exprKind == SyntaxKind.RangeExpression)
		{
			Token comparisonToken = Peek();

			if(comparisonToken.IsComparisonOperator())
			{
				EatToken();
			}
			else
			{
				comparisonToken = default;
			}

			return new RangeExpressionSyntax(left, operatorToken, comparisonToken, ParseExpression(newPrecedence))
			{
				Position = left.Position
			};
		}

		ExpressionSyntax right = ParseExpression(newPrecedence);

		if (operatorToken.IsAssignmentOperator())
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
				return ParseSimpleName();

			case TokenKind.SelfKeyword:
				EatToken();

				return new SelfExpressionSyntax(token)
				{
					Position = token.Position
				};

			case TokenKind.BaseKeyword:
				EatToken();

				return new BaseExpressionSyntax(token)
				{
					Position = token.Position
				};

			case TokenKind.NewKeyword:
				return ParseNewExpression();

			case TokenKind.FalseKeyword:
			case TokenKind.TrueKeyword:
			case TokenKind.NullKeyword:
			case TokenKind.NumericLiteralToken:
			case TokenKind.StringLiteralToken:
			case TokenKind.CharLiteralToken:
				EatToken();

				return new LiteralExpressionSyntax(SyntaxFacts.GetLiteralExpressionKind(token.Kind), token)
				{
					Position = token.Position
				};

			case TokenKind.OpenBracketToken:
				return ParseCollectionExpression();

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
				if (token.IsPredefinedType())
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

				case TokenKind.OpenBracketToken:
					expr = new ElementAccessExpressionSyntax(expr, ParseBracketArgumentList())
					{
						Position = expr.Position
					};

					break;

				case TokenKind.PlusPlusToken:
				case TokenKind.MinusMinusToken:
					expr = new PostfixUnaryExpression(SyntaxFacts.GetPostfixUnaryExpressionKind(token.Kind), expr, EatToken())
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

	private CollectionExpressionSyntax ParseCollectionExpression()
	{
		Token openBracket = EatToken(TokenKind.OpenBracketToken);

		List<(ExpressionSyntax, Token)> expressions = new();

		Token token;

		while((token = Peek()).IsValid())
		{
			if(token.IsKind(TokenKind.CloseBracketToken))
			{
				break;
			}

			ExpressionSyntax expression = ParseExpression();

			token = Peek();

			if(token.IsKind(TokenKind.CommaToken))
			{
				EatToken();
				expressions.Add((expression, token));
				continue;
			}

			expressions.Add((expression, default));
		}

		Token closeBracket = EatToken(TokenKind.CloseBracketToken);
		return new(openBracket, List(expressions), closeBracket)
		{
			Position = openBracket.Position
		};
	}

	private ExpressionSyntax ParseNewExpression()
	{
		Token keyword = EatToken(TokenKind.NewKeyword);

		Token token = Peek();

		switch(token.Kind)
		{
			// Array with no element type.
			case TokenKind.OpenBracketToken:
				{
					SyntaxList<ArrayRankSyntax> ranks = ParseArrayRanks();

					return new ArrayCreationExpressionSyntax(keyword, null, ranks, TryParseInitializer())
					{
						Position = keyword.Position
					};
				}

			// Object with no type, but with argument list.
			case TokenKind.OpenParenToken:
				{
					ArgumentListSyntax argumentList = ParseArgumentList();

					return new ObjectCreationExpressionSyntax(keyword, null, argumentList, TryParseInitializer())
					{
						Position = keyword.Position
					};
				}

			// Anonymous object.
			case TokenKind.OpenBraceToken:
				return new ObjectCreationExpressionSyntax(keyword, null, null, TryParseInitializer())
				{
					Position = keyword.Position
				};

			default:
				{
					TypeSyntax type = ParseType();

					token = Peek();

					if (token.IsKind(TokenKind.OpenBracketToken))
					{
						SyntaxList<ArrayRankSyntax> ranks = ParseArrayRanks();

						return new ArrayCreationExpressionSyntax(keyword, type, ranks, TryParseInitializer())
						{
							Position = keyword.Position
						};
					}

					ArgumentListSyntax argumentList = ParseArgumentList();

					return new ObjectCreationExpressionSyntax(keyword, type, argumentList, TryParseInitializer())
					{
						Position = keyword.Position
					};
				}
		}
	}

	private SyntaxList<ArrayRankSyntax> ParseArrayRanks()
	{
		List<ArrayRankSyntax> ranks = new();

		Token token;

		while((token = Peek()).IsValid())
		{
			if(!token.IsKind(TokenKind.OpenBracketToken))
			{
				break;
			}

			EatToken();

			SeparatedSyntaxList<ExpressionSyntax> sizes = ParseArraySizes();

			Token closeBracket = EatToken(TokenKind.CloseBracketToken);

			ranks.Add(new(token, sizes, closeBracket)
			{
				Position = token.Position
			});
		}

		return List(ranks);
	}

	private SeparatedSyntaxList<ExpressionSyntax> ParseArraySizes()
	{
		List<(ExpressionSyntax, Token)> expressions = new();

		Token token;

		while((token = Peek()).IsValid())
		{
			if(token.IsKind(TokenKind.CloseBracketToken))
			{
				if(expressions.Count == 0)
				{
					Token skippedToken = SkippedToken(token);
					ExpressionSyntax expr = new SkippedArraySizeExpressionSyntax(skippedToken)
					{
						Position = skippedToken.Position,
					};

					expressions.Add((expr, default));
				}

				break;
			}

			if(token.IsKind(TokenKind.CommaToken))
			{
				EatToken();
				Token skippedToken = SkippedToken(token);
				ExpressionSyntax expr = new SkippedArraySizeExpressionSyntax(skippedToken)
				{
					Position = skippedToken.Position,
				};

				expressions.Add((expr, token));
				continue;
			}

			expressions.Add((ParseExpression(), default));
		}

		return List(expressions);
	}

	private InitializerExpressionSyntax? TryParseInitializer()
	{
		Token openBrace = Peek();

		if (!openBrace.IsKind(TokenKind.OpenBraceToken))
		{
			return null;
		}

		List<(ExpressionSyntax, Token)> expressions = new();

		Token token;

		while((token = Peek()).IsValid())
		{
			if(token.IsKind(TokenKind.CloseBraceToken))
			{
				break;
			}

			ExpressionSyntax expression = ParseExpression();

			token = Peek();

			if(token.IsKind(TokenKind.CommaToken))
			{
				EatToken();
				expressions.Add((expression, token));
				continue;
			}

			expressions.Add((expression, default));
		}

		Token closeBrace = EatToken(TokenKind.CloseBraceToken);

		return new(openBrace, List(expressions), closeBrace)
		{
			Position = openBrace.Position
		};
	}

	private ArgumentListSyntax? TryParseArgumentList()
	{
		if(!PeekKind(TokenKind.OpenParenToken))
		{
			return null;
		}

		return ParseArgumentList();
	}

	private BracketArgumentListSyntax ParseBracketArgumentList()
	{
		Token openParen = EatToken(TokenKind.OpenBracketToken);

		List<(ArgumentSyntax, Token)> args = new();

		while (true)
		{
			Token token = Peek();

			if (token.IsKind(TokenKind.CloseBracketToken))
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

		Token closeParen = EatToken(TokenKind.CloseBracketToken);

		return new(openParen, List(args), closeParen)
		{
			Position = openParen.Position
		};
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
				EatToken();
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
		TypeSyntax type = ParseNameOrTypeKeyword();

		if(PeekKind(TokenKind.QuestionToken))
		{
			return new NullableTypeSyntax(type, EatToken())
			{
				Position = type.Position
			};
		}

		if(PeekKind(TokenKind.OpenBracketToken))
		{
			SyntaxList<ArrayRankSyntax> ranks = ParseArrayRanks();

			return new ArrayTypeSyntax(type, ranks)
			{
				Position = type.Position
			};
		}

		return type;

		TypeSyntax ParseNameOrTypeKeyword()
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
	}

	private NameSyntax ParseName()
	{
		NameSyntax name = ParseSimpleName();

		while (PeekKind(TokenKind.DotToken))
		{
			Token dot = EatToken();

			SimpleNameSyntax right = ParseSimpleName();
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

	private static int GetMemberPosition(in SyntaxList<AttributeSyntax> attributes, in TokenList modifiers, in Token token)
	{
		return attributes.IsDefaultOrEmpty
			? modifiers.IsDefaultOrEmpty
				? token.Position
				: modifiers.GetPosition()
			: attributes.GetPosition();
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
			SyntaxKind.NullLiteralExpression or
			SyntaxKind.ParenthesizedExpression or
			SyntaxKind.IdentifierName or
			SyntaxKind.GenericName or
			SyntaxKind.PredefinedType or
			SyntaxKind.InvocationExpression or
			SyntaxKind.PostDecrementExpression or
			SyntaxKind.PostIncrementExpression or
			SyntaxKind.SelfExpression or
			SyntaxKind.BaseExpression or
			SyntaxKind.ArrayCreationExpression or
			SyntaxKind.ObjectCreationExpression or
			SyntaxKind.CollectionExpression or
			SyntaxKind.ElementAccessExpression
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

			SyntaxKind.RangeExpression
				=> Precedence.Range,

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
			SyntaxKind.NotEqualsExpression or
			SyntaxKind.ReferenceEqualsExpression
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

			_ => throw new UnreachableException(),
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
			return MissingTokenWithError(current);
		}

		EatToken();

		return current;
	}

	private void EnsureKind(ref Token token, TokenKind kind)
	{
		if (token.Kind != kind)
		{
			token = MissingTokenWithError(token);
		}
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

	private Token MissingTokenWithError(in Token token)
	{
		if (token.Kind == TokenKind.EOF)
		{
			AddError(ErrorCode.ERR_UnexpectedEndOfFile);
		}
		else
		{
			AddError(ErrorCode.ERR_SyntaxError);
		}

		return MissingToken(token);
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

	private static Token SkippedToken(in Token token)
	{
		return new(TokenKind.SkippedToken, string.Empty, token.Position);
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
		Range,

		// ++a, -a, !a, true/false etc.
		Unary,

		// (int) a
		Cast,

		// a.b, a++, etc.
		Primary
	}
}
