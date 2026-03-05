using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using Zubr.Compiler.Diagnostics;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler.Parser;

internal sealed class SourceParser
{
	private readonly Token[] _tokens;
	private readonly SyntaxTree _tree;
	private int _current;

	private List<InternalDiagnostic>? _errors;

	internal SourceParser(SyntaxTree tree, Token[] tokens) : this(tree, tokens, null)
	{
	}

	internal SourceParser(SyntaxTree tree, Token[] tokens, List<InternalDiagnostic>? errors)
	{
		_tree = tree;
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

		int end = token.Position;

		return new(_tree, new TextSpan(0, end), List(uses), List(aliases), List(members), token);
	}

	internal List<InternalDiagnostic>? GetDiagnostics()
	{
		return _errors;
	}

	private ModuleDeclarationSyntax ParseModuleDeclaration()
	{
		Token moduleKeyword = EatToken();
		int start = moduleKeyword.Position;
		int end;

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

		if(members.Count > 0)
		{
			end = members[^1].Span.End;
		}
		else
		{
			end = semicolonToken.Position;
		}

		TextSpan span = GetSpan(start, end);

		return new(_tree, span, moduleKeyword, topKeyword, name, semicolonToken, List(members));
	}

	private UseDirectiveSyntax ParseUseDirective()
	{
		if(PeekKind(TokenKind.OpenBraceToken, 1))
		{
			return ParseComplexUseDirective();
		}

		return ParseSimpleUseDirective();
	}

	private SimpleUseDirectiveSyntax ParseSimpleUseDirective()
	{
		Token useKeyword = EatToken(TokenKind.UseKeyword);

		NameSyntax name = ParseName();

		Token asKeyword = default;
		IdentifierNameSyntax? alias = null;

		if (PeekKind(TokenKind.AsKeyword))
		{
			asKeyword = EatToken();
			alias = ParseIdentifierName();
		}

		Token semicolon = EatToken(TokenKind.SemicolonToken);
		TextSpan span = GetSpan(useKeyword, semicolon);

		return new(_tree, span, useKeyword, name, asKeyword, alias, semicolon);
	}

	private ListedUseDirectiveSyntax ParseComplexUseDirective()
	{
		Token useKeyword = EatToken(TokenKind.UseKeyword);
		UseDirectiveElementListSyntax elementList = ParseUseDirectiveElementList();

		Token fromKeyword = EatToken(TokenKind.FromKeyword);

		NameSyntax module = ParseName();

		Token semicolon = EatToken(TokenKind.SemicolonToken);

		TextSpan span = GetSpan(useKeyword, semicolon);

		return new(_tree, span, useKeyword, elementList, fromKeyword, module, semicolon);
	}

	private UseDirectiveElementListSyntax ParseUseDirectiveElementList()
	{
		Token openBrace = EatToken(TokenKind.OpenBraceToken);

		if (PeekKind(TokenKind.CloseBraceToken))
		{
			return Close(openBrace, null);
		}

		List<(UseDirectiveElementSyntax, Token)> elements = new();

		while (true)
		{
			UseDirectiveElementSyntax element = ParseUseDirectiveElement();

			ref readonly Token token = ref Peek();

			if(token.IsKind(TokenKind.CloseBraceToken))
			{
				elements.Add((element, default));
				break;
			}

			if(!token.IsKind(TokenKind.CommaToken))
			{
				elements.Add((element, default));
				break;
			}

			elements.Add((element, token));
		}

		return Close(openBrace, elements);

		UseDirectiveElementListSyntax Close(Token openBrace, List<(UseDirectiveElementSyntax, Token)>? elements)
		{
			Token closeBrace = EatToken(TokenKind.CloseBraceToken);

			TextSpan span = GetSpan(openBrace, closeBrace);

			return new(_tree, span, openBrace, elements is null ? default : List(elements), closeBrace);
		}
	}

	private UseDirectiveElementSyntax ParseUseDirectiveElement()
	{
		SimpleNameSyntax name = ParseSimpleName();

		Token asKeyword = default;
		IdentifierNameSyntax? alias = null;

		if(PeekKind(TokenKind.AsKeyword))
		{
			asKeyword = EatToken();
			alias = ParseIdentifierName();
		}

		TextSpan span = alias is null
			? name.Span
			: GetSpan(name, alias);

		return new(_tree, span, name, asKeyword, alias);
	}

	private AliasDirectiveSyntax ParseAliasDirective()
	{
		TokenList modifiers = ParseModifiers();

		Token keyword = EatToken(TokenKind.AliasKeyword);
		SimpleNameSyntax alias = ParseSimpleName();

		Token equalsToken = EatToken(TokenKind.EqualsToken);

		NameSyntax name = ParseName();

		Token semicolonToken = EatToken(TokenKind.SemicolonToken);

		int start = modifiers.IsDefaultOrEmpty
			? keyword.Position
			: modifiers.Span.Start;

		TextSpan span = GetSpan(start, semicolonToken);

		return new(_tree, span, modifiers, keyword, alias, equalsToken, name, semicolonToken);
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

			return token.ContextualKind switch
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
		TextSpan span = GetSpan(position, semicolonToken);

		return new(_tree, span, attributes, modifiers, fieldKeyword, variable, semicolonToken);
	}

	private ConstructorDeclarationSyntax ParseConstructorDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		Token keyword = EatToken(TokenKind.NewKeyword);

		ParameterListSyntax parameterList = ParseParameterList();

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody(out int end);

		TextSpan span = GetSpan(position, end);
		return new(_tree, span, attributes, modifiers, keyword, parameterList, body, expressionBody, semicolonToken);
	}

	private DestructorDeclarationSyntax ParseDestructorDeclaration(SyntaxList<AttributeSyntax> attributes, TokenList modifiers, int position)
	{
		Token keyword = EatContextualKeyword();

		ParameterListSyntax parameterList = ParseParameterList();

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody(out int end);

		TextSpan span = GetSpan(position, end);

		return new(_tree, span, attributes, modifiers, keyword, parameterList, body, expressionBody, semicolonToken);
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
		TextSpan span;

		if (token.IsKind(TokenKind.EqualsGreaterThanToken))
		{
			EatToken();

			ExpressionSyntax expr = ParseExpression();
			expressionBody = new(_tree, GetSpan(token, expr), token, expr);

			semicolonToken = EatToken(TokenKind.SemicolonToken);

			initializer = null;
			accessorList = null;

			span = GetSpan(position, semicolonToken);
		}
		else
		{
			accessorList = TryParseAccesorList();
			initializer = TryParseEqualsValueClause();
			expressionBody = null;

			if (accessorList is null)
			{
				semicolonToken = EatToken(TokenKind.SemicolonToken);
				span = GetSpan(position, semicolonToken);
			}
			else
			{
				semicolonToken = default;
				span = initializer is null
					? GetSpan(position, accessorList)
					: GetSpan(position, initializer);
			}
		}

		return new(_tree, span, attributes, modifiers, returnType, identifier, expressionBody, accessorList, initializer, semicolonToken);
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
		TextSpan span;

		if (token.IsKind(TokenKind.EqualsGreaterThanToken))
		{
			EatToken();

			ExpressionSyntax expr = ParseExpression();
			expressionBody = new(_tree, GetSpan(token, expr), token, expr);

			semicolonToken = EatToken(TokenKind.SemicolonToken);

			accessorList = null;

			span = GetSpan(position, semicolonToken);
		}
		else
		{
			accessorList = TryParseAccesorList();
			expressionBody = null;

			if (accessorList is null)
			{
				semicolonToken = EatToken(TokenKind.SemicolonToken);
				span = GetSpan(position, semicolonToken);
			}
			else
			{
				semicolonToken = default;
				span = GetSpan(position, accessorList);
			}
		}

		return new(_tree, span, attributes, modifiers, returnType, selfKeyword, parameterList, expressionBody, accessorList, semicolonToken);
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

			Token keyword = EatContextualKeyword();

			int position = GetMemberPosition(attributes, modifiers, keyword);

			SyntaxKind kind = keyword.GetAccessorKind();

			if(kind == default)
			{
				AddError(ErrorCode.ERR_SyntaxError);
				keyword = UnexpectedToken();
			}

			(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody(out int end);

			accessors.Add(new(_tree, GetSpan(position, end), kind, attributes, modifiers, keyword, body, expressionBody, semicolonToken));
		}

		Token closeBrace = EatToken(TokenKind.CloseBraceToken);

		TextSpan span = GetSpan(openBrace, closeBrace);

		return new(_tree, span, openBrace, List(accessors), closeBrace);
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

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody(out int end);

		TextSpan span = GetSpan(position, end);

		return new(_tree, span, attributes, modifiers, keyword, type, parameterList, body, expressionBody, semicolonToken);
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

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody(out int end);

		TextSpan span = GetSpan(position, end);

		return new(_tree, span, attributes, modifiers, returnType, keyword, token, parameterList, body, expressionBody, semicolonToken);
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

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody(out int end);

		TextSpan span = GetSpan(position, end);

		return new(_tree, span, attributes, modifiers, returnType, selfKeyword, parameterList, body, expressionBody, semicolonToken);
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

		(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody, Token semicolonToken) = ParseBody(out int end);

		TextSpan span = GetSpan(position, end);

		return new(_tree, span, attributes, modifiers, returnType, identifier, typeParameterList, parameterList, constraintList, body, expressionBody, semicolonToken);
	}

	private TypeParameterListSyntax? TryParseTypeParameterList()
	{
		if (!PeekKind(TokenKind.LessThanToken))
		{
			return null;
		}

		Token lessThanToken = EatToken();

		List<(TypeParameterSyntax, Token)> parameters = new();

		while (true)
		{
			TypeParameterSyntax typeParameter = ParseTypeParameter();

			Token token = Peek();

			if (token.IsKind(TokenKind.GreaterThanToken))
			{
				parameters.Add((typeParameter, default));
				break;
			}

			if (!EatToken(TokenKind.CommaToken, out token))
			{
				parameters.Add((typeParameter, default));
				break;
			}

			parameters.Add((typeParameter, token));
		}

		Token greaterThanToken = EatToken(TokenKind.GreaterThanToken);

		TextSpan span = GetSpan(lessThanToken, greaterThanToken);

		return new(_tree, span, lessThanToken, List(parameters), greaterThanToken);
	}

	private TypeParameterSyntax ParseTypeParameter()
	{
		SyntaxList<AttributeSyntax> attributes = ParseAttributes();
		Token identifier = EatToken(TokenKind.IdentifierToken);

		TypeParameterInlineConstraintSyntax? constraint = null;
		EqualsTypeClauseSyntax? defaultType = null;

		if (PeekKind(TokenKind.ColonToken))
		{
			Token colonToken = EatToken();

			if(TryParseConstraint() is not TypeParameterConstraintSyntax innerConstraint)
			{
				AddError(ErrorCode.ERR_SyntaxError);
				EatToken();
				constraint = null;
			}
			else
			{
				constraint = new(_tree, GetSpan(colonToken, innerConstraint), colonToken, innerConstraint);
			}
		}

		if(PeekKind(TokenKind.EqualsToken))
		{
			Token equalsToken = EatToken();
			NameSyntax name = ParseName();

			defaultType = new(_tree, GetSpan(equalsToken, name), equalsToken, name);
		}

		TextSpan span;

		if(defaultType is not null)
		{
			span = GetSpan(identifier, defaultType);
		}
		else if(constraint is not null)
		{
			span = GetSpan(identifier, constraint);
		}
		else
		{
			span = Peek().Span;
		}

		return new(_tree, span, attributes, identifier, constraint, defaultType);
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
				if (TryParseConstraint() is not TypeParameterConstraintSyntax constraint)
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

			TypeParameterConstraintClauseSyntax clause = new(_tree, GetSpan(identifier, constraints), identifier, colonToken, List(constraints));

			clauses.Add((clause, commaToken));

			// End of the constraint clause list.
			if (!Peek().IsKind(TokenKind.IdentifierToken))
			{
				break;
			}
		}

		TextSpan span = GetSpan(whereKeyword, clauses);

		return new(_tree, span, whereKeyword, List(clauses));
	}

	private TypeParameterConstraintSyntax? TryParseConstraint()
	{
		Token token = Peek();

		TokenKind kind = token.ContextualKind;

		switch (kind)
		{
			case TokenKind.ClassKeyword:
				return Keyword(token, SyntaxKind.ClassConstraint, true);

			case TokenKind.StructKeyword:
				return Keyword(token, SyntaxKind.StructConstraint, true);

			case TokenKind.EnumKeyword:
				return Keyword(token, SyntaxKind.EnumConstraint, true);

			case TokenKind.UnmanagedKeyword:
				ChangeKind(ref token, TokenKind.UnmanagedKeyword);
				return Keyword(token, SyntaxKind.UnmanagedConstraint, false);

			case TokenKind.IdentifierToken:
				NameSyntax name = ParseName();
				return new TypeConstraintSyntax(_tree, name.Span, name);

			default:
				return null;
		}

		KeywordConstraintSyntax Keyword(in Token token, SyntaxKind kind, bool allowQuestion)
		{
			EatToken();

			Token questionToken;
			
			if(PeekKind(TokenKind.QuestionToken))
			{
				if(allowQuestion)
				{
					questionToken = EatToken();
				}
				else
				{
					AddError(ErrorCode.ERR_SyntaxError);
					questionToken = UnexpectedToken();
				}
			}
			else
			{
				questionToken = default;
			}

			return new KeywordConstraintSyntax(_tree, token.Span, kind, token, questionToken);
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

		TextSpan span = GetSpan(openBracket, token);

		return new(_tree, span, openBracket, List(parameters), token);
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

		TextSpan span = GetSpan(openParen, token);

		return new(_tree, span, openParen, List(parameters), token);
	}

	private ParameterSyntax ParseParameter()
	{
		SyntaxList<AttributeSyntax> attributes = ParseAttributes();
		TokenList modifiers = ParseModifiers();

		TypeSyntax type = ParseType();

		Token identifier = EatToken(TokenKind.IdentifierToken);

		int position = GetMemberPosition(attributes, modifiers, identifier);

		EqualsValueClauseSyntax? @default = null;

		TextSpan span;

		if (PeekKind(TokenKind.EqualsToken))
		{
			Token equalsToken = EatToken();

			ExpressionSyntax value = ParseExpression();

			@default = new(_tree, GetSpan(equalsToken, value), equalsToken, value);

			span = GetSpan(position, @default);
		}
		else
		{
			span = GetSpan(position, identifier);
		}

		return new(_tree, span, attributes, modifiers, type, identifier, @default);
	}

	private TokenList ParseModifiers()
	{
		if(!Peek().IsModifier())
		{
			return TokenList.Empty;
		}

		Token token = EatContextualKeyword();

		List<Token> tokens = new()
		{
			token
		};

		while(Peek().IsModifier())
		{
			token = EatContextualKeyword();
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
		TextSpan span;

		if (PeekKind(TokenKind.SemicolonToken))
		{
			semicolonToken = EatToken();
			openBrace = default;
			members = default;
			closeBrace = default;
			span = GetSpan(position, semicolonToken);
		}
		else
		{
			openBrace = EatToken(TokenKind.OpenBraceToken);
			members = ParseTypeMembers();
			closeBrace = EatToken(TokenKind.CloseBraceToken);
			semicolonToken = default;
			span = GetSpan(position, closeBrace);
		}

		return new(_tree, span, attributes, modifiers, keyword, typeParameterList, type, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace);
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
		TextSpan span;

		if (PeekKind(TokenKind.SemicolonToken))
		{
			semicolonToken = EatToken();
			openBrace = default;
			members = default;
			closeBrace = default;
			span = GetSpan(position, semicolonToken);
		}
		else
		{
			openBrace = EatToken(TokenKind.OpenBraceToken);
			members = ParseEnumMembers();
			closeBrace = EatToken(TokenKind.CloseBraceToken);
			semicolonToken = default;
			span = GetSpan(position, closeBrace);
		}

		if (nextKeyword.IsKind(TokenKind.ClassKeyword))
		{
			return new EnumClassDeclarationSyntax(_tree, span, attributes, modifiers, keyword, nextKeyword, identifier, parameterList, baseTypeList, semicolonToken, openBrace, members, closeBrace);
		}

		if(nextKeyword.IsKind(TokenKind.StructKeyword))
		{
			return new EnumStructDeclarationSyntax(_tree, span, attributes, modifiers, keyword, nextKeyword, identifier, parameterList, baseTypeList, semicolonToken, openBrace, members, closeBrace);
		}

		return new SimpleEnumDeclarationSyntax(_tree, span, attributes, modifiers, keyword, identifier, semicolonToken, openBrace, members, closeBrace);
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

		if (argumentList is null)
		{
			EqualsValueClauseSyntax? initializer = TryParseEqualsValueClause();

			TextSpan span = initializer is null
				? GetSpan(position, identifier)
				: GetSpan(position, initializer);

			return new SimpleEnumMemberDeclarationSyntax(_tree, span, attributes, modifiers, identifier, initializer);
		}
		else
		{
			TextSpan span = GetSpan(position, argumentList);

			return new ComplexEnumMemberDeclarationSyntax(_tree, span, attributes, modifiers, identifier, argumentList);
		}
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
		TextSpan span;

		if (PeekKind(TokenKind.SemicolonToken))
		{
			semicolonToken = EatToken();
			openBrace = default;
			members = default;
			closeBrace = default;
			span = GetSpan(position, semicolonToken);
		}
		else
		{
			openBrace = EatToken(TokenKind.OpenBraceToken);
			members = ParseTypeMembers();
			closeBrace = EatToken(TokenKind.CloseBraceToken);
			semicolonToken = default;
			span = GetSpan(position, closeBrace);
		}

		return kind switch
		{
			SyntaxKind.ClassDeclaration => new ClassDeclarationSyntax(_tree, span, attributes, modifiers, keyword, identifier, typeParameterList, parameterList, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace),

			SyntaxKind.StructDeclaration => new StructDeclarationSyntax(_tree, span, attributes, modifiers, keyword, identifier, typeParameterList, parameterList, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace),

			SyntaxKind.TraitDeclaration => new TraitDeclarationSyntax(_tree, span, attributes, modifiers, keyword, identifier, typeParameterList, parameterList, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace),

			SyntaxKind.AttributeDeclaration => new AttributeDeclarationSyntax(_tree, span, attributes, modifiers, keyword, identifier, typeParameterList, parameterList, baseTypeList, constraints, semicolonToken, openBrace, members, closeBrace),

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

		TextSpan span = GetSpan(colonToken, baseTypes);

		return new(_tree, span, colonToken, List(baseTypes));

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
			return new SimpleBaseTypeSyntax(_tree, type.Span, type);
		}

		TextSpan span = GetSpan(type, argumentList);
		return new PrimaryBaseTypeSyntax(_tree, span, type, argumentList);
	}

	private (BlockSyntax? block, ArrowExpressionClauseSyntax? expression, Token semicolonToken) ParseBody(out int end)
	{
		ref readonly Token token = ref Peek();

		switch(token.Kind)
		{
			case TokenKind.OpenBraceToken:
				BlockSyntax block = ParseBlock();
				end = block.Span.End;
				return (block, null, default);

			case TokenKind.EqualsGreaterThanToken:
				EatToken();
				ExpressionSyntax expr = ParseExpression();
				TextSpan span = GetSpan(token, expr);
				ArrowExpressionClauseSyntax exprBody = new(_tree, span, token, expr);

				Token semicolonToken = EatToken(TokenKind.SemicolonToken);
				end = semicolonToken.Span.End;
				return (null, exprBody, semicolonToken);

			case TokenKind.SemicolonToken:
				EatToken();
				end = token.Span.End;
				return (null, null, token);

			default:
				end = token.Span.End;
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
		TextSpan span = GetSpan(openBrace, closeBrace);

		return new(_tree, span, openBrace, List(statements), closeBrace);
	}

	private StatementSyntax ParseStatement()
	{
		Token token = Peek();

		return token.ContextualKind switch
		{
			TokenKind.OpenBraceToken => ParseBlock(),
			TokenKind.IfKeyword => ParseIfStatement(),
			TokenKind.WhileKeyword => ParseWhileStatement(),
			TokenKind.DoKeyword => ParseDoStatement(),
			TokenKind.ForKeyword => ParseForStatement(),
			TokenKind.GotoKeyword => ParseGotoStatement(),
			TokenKind.IdentifierToken => ParseLocalOrExpressionStatement(),
			TokenKind.ReturnKeyword => ParseReturnStatement(),
			TokenKind.NextKeyword => ParseNextStatement(),
			TokenKind.StopKeyword => ParseStopStatement(),
			TokenKind.UnsafeKeyword => ParseUnsafeStatement(),
			TokenKind.LockKeyword => ParseLockStatement(),
			_ => ParseLocalOrExpressionStatement(),
		};
	}

	private LockStatementSyntax ParseLockStatement()
	{
		Token keyword = EatToken(TokenKind.LockKeyword);

		Token openParen = EatToken(TokenKind.OpenParenToken);
		ExpressionSyntax expr = ParseExpression();
		Token closeParen = EatToken(TokenKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		TextSpan span = GetSpan(keyword, statement);

		return new(_tree, span, keyword, openParen, expr, closeParen, statement);
	}

	private UnsafeStatementSyntax ParseUnsafeStatement()
	{
		Token unsafeKeyword = EatToken(TokenKind.UnsafeKeyword);

		BlockSyntax block = ParseBlock();

		return new(_tree, GetSpan(unsafeKeyword, block), unsafeKeyword, block);
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

		TextSpan span = GetSpan(returnKeyword, semicolon);

		return new(_tree, span, returnKeyword, expression, semicolon);
	}

	private NextStatementSyntax ParseNextStatement()
	{
		Token nextKeyword = EatContextualKeyword(TokenKind.NextKeyword);
		Token semicolon = EatToken(TokenKind.SemicolonToken);

		TextSpan span = GetSpan(nextKeyword, semicolon);

		return new(_tree, span, nextKeyword, semicolon);
	}

	private StopStatementSyntax ParseStopStatement()
	{
		Token stopKeyword = EatContextualKeyword(TokenKind.StopKeyword);
		Token semicolon = EatToken(TokenKind.SemicolonToken);

		TextSpan span = GetSpan(stopKeyword, semicolon);

		return new(_tree, span, stopKeyword, semicolon);
	}

	private StatementSyntax ParseLocalOrExpressionStatement()
	{
		Token token = Peek();

		if (token.IsKind(TokenKind.IdentifierToken) && PeekKind(TokenKind.ColonToken, 1))
		{
			return ParseLabelStatement();
		}

		if (token.IsPredefinedType())
		{
			if(!PeekKind(TokenKind.DotToken, 1))
			{
				return ParseLocalDeclaration();
			}

			if(PeekKind(TokenKind.ColonToken, 1))
			{
				return ParseLabelStatement();
			}
		}

		ExpressionSyntax expr = ParseExpression();
		Token semicolonToken = EatToken();

		TextSpan span = GetSpan(token, semicolonToken);

		return new ExpressionStatementSyntax(_tree, span, expr, semicolonToken);
	}

	private LocalDeclarationStatementSyntax ParseLocalDeclaration()
	{
		TokenList modifiers = ParseModifiers();

		VariableDeclarationSyntax variable = ParseVariable();

		int position = modifiers.IsDefaultOrEmpty
			? variable.Position
			: modifiers.Position;

		Token semicolon = EatToken(TokenKind.SemicolonToken);

		if (variable.Type is PredefinedTypeSyntax p && p.Keyword.IsKind(TokenKind.VoidKeyword))
		{
			AddError(ErrorCode.ERR_SyntaxError);
		}

		TextSpan span = GetSpan(position, semicolon);

		return new(_tree, span, modifiers, variable, semicolon);
	}

	private VariableDeclarationSyntax ParseVariable()
	{
		TypeSyntax type = ParseType();

		Token identifier = EatToken(TokenKind.IdentifierToken);

		EqualsValueClauseSyntax? initializer = TryParseEqualsValueClause();

		TextSpan span = initializer is null
			? GetSpan(type, identifier)
			: GetSpan(type, initializer);

		return new(_tree, span, type, identifier, initializer);
	}

	private EqualsValueClauseSyntax? TryParseEqualsValueClause()
	{
		if (!PeekKind(TokenKind.EqualsToken))
		{
			return null;
		}

		Token equalsToken = EatToken();
		ExpressionSyntax expr = ParseExpression();

		TextSpan span = GetSpan(equalsToken, expr);

		return new(_tree, span, equalsToken, expr);
	}

	private WhileStatementSyntax ParseWhileStatement()
	{
		Token whileKeyword = EatToken(TokenKind.WhileKeyword);
		Token openParen = EatToken(TokenKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		Token closeParen = EatToken(TokenKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		TextSpan span = GetSpan(whileKeyword, statement);

		return new(_tree, span, whileKeyword, openParen, condition, closeParen, statement);
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

		TextSpan span = GetSpan(doKeyword, semicolon);

		return new(_tree, span, doKeyword, statement, whileKeyword, openParen, condition, closeParen, semicolon);
	}

	private GotoStatementSyntax ParseGotoStatement()
	{
		Token keyword = EatToken(TokenKind.GotoKeyword);

		Token identifier = EatToken(TokenKind.IdentifierToken);

		Token semicolon = EatToken(TokenKind.SemicolonToken);

		TextSpan span = GetSpan(keyword, semicolon);
		return new(_tree, span, keyword, identifier, semicolon);
	}

	private LabelStatementSyntax ParseLabelStatement()
	{
		Token identifier = EatToken(TokenKind.IdentifierToken);

		Token colon = EatToken(TokenKind.ColonToken);

		StatementSyntax statement = ParseStatement();

		TextSpan span = GetSpan(identifier, statement);

		return new(_tree, span, identifier, colon, statement);
	}

	private ForStatementSyntax ParseForStatement()
	{
		Token forKeyword = EatToken(TokenKind.ForKeyword);

		Token openParen = EatToken(TokenKind.OpenParenToken);
		VariableExpressionSyntax variable = ParseVariableExpression();

		Token colon = EatToken(TokenKind.ColonToken);

		ExpressionSyntax expression = ParseExpression();

		Token closeParen = EatToken(TokenKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		TextSpan span = GetSpan(forKeyword, statement);

		return new(_tree, span, forKeyword, openParen, variable, colon, expression, closeParen, statement);
	}

	private VariableExpressionSyntax ParseVariableExpression()
	{
		TypeSyntax type = ParseType();

		if(type.IsKind(SyntaxKind.IdentifierName))
		{
			// The type is actually a typeless variable.
			if(!PeekKind(TokenKind.IdentifierToken))
			{
				return new(_tree, type.Span, null, (type as IdentifierNameSyntax)!.Identifier);
			}
		}

		Token identifier = EatToken();

		TextSpan span = GetSpan(type, identifier);

		return new(_tree, span, type, identifier);
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
			@else = ParseElseClause();
		}

		TextSpan span;

		if(@else is not null)
		{
			span = GetSpan(ifKeyword, @else);
		}
		else if(elifs is not null)
		{
			span = GetSpan(ifKeyword, elifs);
		}
		else
		{
			span = GetSpan(ifKeyword, statement);
		}

		return new(_tree, span, ifKeyword, openParen, condition, closeParen, statement, ListIfNotNull(elifs), @else);
	}

	private ElifClauseSyntax ParseElifClause()
	{
		Token elifKeyword = EatToken(TokenKind.ElifKeyword);
		Token openParen = EatToken(TokenKind.OpenParenToken);

		ExpressionSyntax condition = ParseExpression();

		Token closeParen = EatToken(TokenKind.CloseParenToken);

		StatementSyntax statement = ParseStatement();

		TextSpan span = GetSpan(elifKeyword, statement);

		return new(_tree, span, elifKeyword, openParen, condition, closeParen, statement);
	}

	private ElseClauseSyntax ParseElseClause()
	{
		Token elseKeyword = EatToken();

		StatementSyntax statement = ParseStatement();

		if (statement.IsKind(SyntaxKind.IfStatement))
		{
			AddError(ErrorCode.ERR_ElseIfNotSupported, statement.Position);
		}

		TextSpan span = GetSpan(elseKeyword, statement);

		return new(_tree, span, elseKeyword, statement);
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

		TextSpan span = GetSpan(openBracket, closeBracket);

		return new(_tree, span, openBracket, target, name, argumentList, closeBracket);
	}

	private AttributeTargetSyntax? TryParseAttributeTarget()
	{
		Token token = Peek();

		TokenKind kind = token.ContextualKind;

		// TODO: Handle all attribute target specifiers.
		switch (kind)
		{
			case TokenKind.ReturnKeyword:
			case TokenKind.AssemblyKeyword:
			case TokenKind.FieldKeyword:
				Token targetKeyword = AcceptContextualKeyword(kind);
				Token colonToken = EatToken(TokenKind.ColonToken);

				TextSpan span = GetSpan(targetKeyword, colonToken);

				return new(_tree, span, targetKeyword, colonToken);

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
		TextSpan span = GetSpan(openParen, closeParen);

		return new(_tree, span, openParen, List(args), closeParen);
	}

	private AttributeArgumentSyntax ParseAttributeArgument()
	{
		ExpressionSyntax expr = ParseExpression();

		return new(_tree, expr.Span, expr);
	}

	private ExpressionSyntax ParseExpression(Precedence precedence = default)
	{
		Token token = Peek();
		SyntaxKind kind = SyntaxFacts.GetPrefixUnaryExpressionKind(token.Kind);

		ExpressionSyntax expr;

		if (kind != default)
		{
			EatToken();
			expr = ParseExpression(GetPrecedence(kind));
			TextSpan span = GetSpan(token, expr);
			return new PrefixUnaryExpressionSyntax(_tree, span, kind, token, expr);
		}

		ExpressionSyntax primary = ParsePrimaryExpression();
		primary = ParsePostfixExpression(primary);

		expr = primary;

		while (TryParseSubExpression(expr, precedence) is ExpressionSyntax sub)
		{
			expr = sub;
		}

		if (PeekKind(TokenKind.QuestionToken) && precedence <= Precedence.Conditional)
		{
			Token questionToken = EatToken();
			ExpressionSyntax trueExpression = ParseExpression();

			Token colonToken = EatToken(TokenKind.ColonToken);
			ExpressionSyntax falseExpression = ParseExpression();

			TextSpan span = GetSpan(expr, falseExpression);

			expr = new ConditionalExpressionSyntax(_tree, span, expr, questionToken, trueExpression, colonToken, falseExpression);
		}

		return expr;
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

		if(exprKind == SyntaxKind.RangeExpression)
		{
			return ParseRangeExpression(left, newPrecedence);
		}

		Token operatorToken = EatToken();

		ExpressionSyntax right = ParseExpression(newPrecedence);

		TextSpan span = GetSpan(left, right);

		if (operatorToken.IsAssignmentOperator())
		{
			return new AssignmentExpressionSyntax(_tree, span, left, operatorToken, right);
		}

		return new BinaryExpressionSyntax(_tree, span, exprKind, left, operatorToken, right);
	}

	private RangeExpressionSyntax ParseRangeExpression(ExpressionSyntax left, Precedence newPrecedence)
	{
		Token operatorToken = EatToken(TokenKind.DotDotToken);

		Token comparisonToken = Peek();

		if (comparisonToken.IsComparisonOperator())
		{
			EatToken();
		}
		else
		{
			comparisonToken = default;
		}

		ExpressionSyntax right = ParseExpression(newPrecedence);

		TextSpan span = GetSpan(left, right);

		return new RangeExpressionSyntax(_tree, span, left, operatorToken, comparisonToken, right);
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

				return new SelfExpressionSyntax(_tree, token.Span, token);

			case TokenKind.BaseKeyword:
				EatToken();

				return new BaseExpressionSyntax(_tree, token.Span, token);

			case TokenKind.NewKeyword:
				return ParseNewExpression();

			case TokenKind.FalseKeyword:
			case TokenKind.TrueKeyword:
			case TokenKind.NullKeyword:
			case TokenKind.NumericLiteralToken:
			case TokenKind.StringLiteralToken:
			case TokenKind.CharLiteralToken:
				EatToken();

				return new LiteralExpressionSyntax(_tree, token.Span, SyntaxFacts.GetLiteralExpressionKind(token.Kind), token);

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

				TextSpan span = GetSpan(openParen, closeParen);

				return new ParenthesizedExpressionSyntax(_tree, span, openParen, expr, closeParen);

			default:
				if (token.IsPredefinedType())
				{
					return new PredefinedTypeSyntax(_tree, token.Span, token);
				}

				return new IdentifierNameSyntax(_tree, token.Span, MissingToken(token));
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
					{
						ArgumentListSyntax argumentList = ParseArgumentList();
						TextSpan span = GetSpan(expr, argumentList);
						expr = new InvocationExpressionSyntax(_tree, span, expr, argumentList);
					}

					break;

				case TokenKind.OpenBracketToken:
					{
						BracketArgumentListSyntax argumentList = ParseBracketArgumentList();
						TextSpan span = GetSpan(expr, argumentList);
						expr = new ElementAccessExpressionSyntax(_tree, span, expr, argumentList);
					}

					break;

				case TokenKind.PlusPlusToken:
				case TokenKind.MinusMinusToken:
					{
						TextSpan span = GetSpan(expr, token);
						expr = new PostfixUnaryExpressionSyntax(_tree, span, SyntaxFacts.GetPostfixUnaryExpressionKind(token.Kind), expr, EatToken());
					}

					break;

				case TokenKind.DotToken:
					{
						EatToken();

						SimpleNameSyntax name = ParseSimpleName();
						TextSpan span = GetSpan(expr, name);
						expr = new MemberAccessExpressionSyntax(_tree, span, SyntaxKind.SimpleMemberAccessExpression, expr, token, name);
					}

					break;

				case TokenKind.MinutGreaterThanToken:
					{
						EatToken();

						SimpleNameSyntax name = ParseSimpleName();
						TextSpan span = GetSpan(expr, name);
						expr = new MemberAccessExpressionSyntax(_tree, span, SyntaxKind.PointerMemberAccessExpression, expr, token, name);
					}

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

		TextSpan span = GetSpan(openBracket, closeBracket);

		return new(_tree, span, openBracket, List(expressions), closeBracket);
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

					InitializerExpressionSyntax? initializer = TryParseInitializer();

					TextSpan span = initializer is null
						? GetSpan(keyword, ranks)
						: GetSpan(keyword, initializer);

					return new ArrayCreationExpressionSyntax(_tree, span, keyword, null, ranks, initializer);
				}

			// Object with no type, but with argument list.
			case TokenKind.OpenParenToken:
				{
					ArgumentListSyntax argumentList = ParseArgumentList();
					InitializerExpressionSyntax? initializer = TryParseInitializer();

					TextSpan span = initializer is null
						? GetSpan(keyword, argumentList)
						: GetSpan(keyword, initializer);

					return new ObjectCreationExpressionSyntax(_tree, span, keyword, null, argumentList, initializer);
				}

			// Anonymous object.
			case TokenKind.OpenBraceToken:
				{
					InitializerExpressionSyntax? initializer = TryParseInitializer();

					TextSpan span = initializer is null
						? keyword.Span
						: GetSpan(keyword, initializer);

					return new ObjectCreationExpressionSyntax(_tree, span, keyword, null, null, initializer);
				}

			default:
				{
					TypeSyntax type = ParseType();

					token = Peek();

					if (token.IsKind(TokenKind.OpenBracketToken))
					{
						SyntaxList<ArrayRankSyntax> ranks = ParseArrayRanks();

						InitializerExpressionSyntax? initializer = TryParseInitializer();

						TextSpan span = initializer is null
							? GetSpan(keyword, ranks)
							: GetSpan(keyword, initializer);

						return new ArrayCreationExpressionSyntax(_tree, span, keyword, type, ranks, initializer);
					}
					else
					{
						ArgumentListSyntax argumentList = ParseArgumentList();

						InitializerExpressionSyntax? initializer = TryParseInitializer();

						TextSpan span = initializer is null
							? GetSpan(keyword, argumentList)
							: GetSpan(keyword, initializer);

						return new ObjectCreationExpressionSyntax(_tree, span, keyword, type, argumentList, initializer);
					}
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

			TextSpan span = GetSpan(token, closeBracket);

			ranks.Add(new(_tree, span, token, sizes, closeBracket));
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
					TextSpan span = skippedToken.Span;
					ExpressionSyntax expr = new SkippedArraySizeExpressionSyntax(_tree, span, skippedToken);

					expressions.Add((expr, default));
				}

				break;
			}

			if(token.IsKind(TokenKind.CommaToken))
			{
				EatToken();
				Token skippedToken = SkippedToken(token);
				TextSpan span = skippedToken.Span;
				ExpressionSyntax expr = new SkippedArraySizeExpressionSyntax(_tree, span, skippedToken);

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

		TextSpan span = GetSpan(openBrace, closeBrace);

		return new(_tree, span, openBrace, List(expressions), closeBrace);
	}

	private ArgumentListSyntax? TryParseArgumentList()
	{
		if(!PeekKind(TokenKind.OpenParenToken))
		{
			return null;
		}

		return ParseArgumentList();
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

		TextSpan span = GetSpan(openParen, closeParen);

		return new(_tree, span, openParen, List(args), closeParen);
	}

	private BracketArgumentListSyntax ParseBracketArgumentList()
	{
		Token openBracket = EatToken(TokenKind.OpenBracketToken);

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

		Token closeBracket = EatToken(TokenKind.CloseBracketToken);

		TextSpan span = GetSpan(openBracket, closeBracket);

		return new(_tree, span, openBracket, List(args), closeBracket);
	}

	private ArgumentSyntax ParseArgument()
	{
		ExpressionSyntax expr = ParseExpression();

		TextSpan span = expr.Span;

		return new(_tree, span, expr);
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

		TextSpan span = GetSpan(openParen, expr);

		return new(_tree, span, openParen, type, closeParen, expr);
	}

	private TypeSyntax ParseType()
	{
		TypeSyntax type = ParseNameOrTypeKeyword();

		ref readonly Token token = ref Peek();

		switch(token.Kind)
		{
			case TokenKind.QuestionToken:
				{
					TextSpan span = GetSpan(type, token);
					return new NullableTypeSyntax(_tree, span, type, token);
				}

			case TokenKind.AsteriskToken:
				{
					TextSpan span = GetSpan(type, token);
					return new PointerTypeSyntax(_tree, span, type, token);
				}

			case TokenKind.AmpersandToken:
				{
					TextSpan span = GetSpan(type, token);
					return new ReferenceTypeSyntax(_tree, span, type, token);
				}

			case TokenKind.OpenBracketToken:
				{
					SyntaxList<ArrayRankSyntax> ranks = ParseArrayRanks();

					TextSpan span = GetSpan(type, ranks);

					return new ArrayTypeSyntax(_tree, span, type, ranks);
				}

			default:
				return type;
		}

		TypeSyntax ParseNameOrTypeKeyword()
		{
			Token token = Peek();

			if (token.IsPredefinedType())
			{
				return ParsePredefinedType();
			}

			if (token.IsKind(TokenKind.LetKeyword))
			{
				return ParseLetType();
			}

			return ParseName();
		}
	}

	private LetTypeSyntax ParseLetType()
	{
		Token keyword = EatToken(TokenKind.LetKeyword);
		return new(_tree, keyword.Span, keyword);
	}

	private PredefinedTypeSyntax ParsePredefinedType()
	{
		Token token = EatToken();

		TextSpan span = token.Span;
		return new(_tree, span, token);
	}

	private NameSyntax ParseName()
	{
		NameSyntax name = ParseSimpleName();

		while (PeekKind(TokenKind.DotToken))
		{
			Token dot = EatToken();

			SimpleNameSyntax right = ParseSimpleName();

			TextSpan span = GetSpan(name, right);

			name = new QualifiedNameSyntax(_tree, span, name, dot, right);
		}

		return name;
	}

	private SimpleNameSyntax ParseSimpleName()
	{
		if (PeekKind(TokenKind.LessThanToken, 1))
		{
			return ParseGenericName();
		}

		return ParseIdentifierName();
	}

	private GenericNameSyntax ParseGenericName()
	{
		Token identifier = EatToken(TokenKind.IdentifierToken);
		TypeArgumentListSyntax list = ParseTypeArgumentList();

		TextSpan span = GetSpan(identifier, list);

		return new GenericNameSyntax(_tree, span, identifier, list);
	}

	private TypeArgumentListSyntax ParseTypeArgumentList()
	{
		Token lessThanToken = EatToken(TokenKind.LessThanToken);

		List<(TypeSyntax, Token)> args = new();

		while (true)
		{
			TypeSyntax type;

			if (PeekKind(TokenKind.GreaterThanToken) || PeekKind(TokenKind.CommaToken))
			{
				type = SkippedTypeArgument();
			}
			else
			{
				type = ParseType();
			}

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

		TextSpan span = GetSpan(lessThanToken, greaterThanToken);

		return new(_tree, span, lessThanToken, List(args), greaterThanToken);

		TypeSyntax SkippedTypeArgument()
		{
			Token skippedToken = SkippedToken();
			return new SkippedTypeArgumentSyntax(_tree, skippedToken.Span, skippedToken);
		}
	}

	private IdentifierNameSyntax ParseIdentifierName()
	{
		Token identifier = EatToken(TokenKind.IdentifierToken);

		TextSpan span = identifier.Span;

		return new(_tree, span, identifier);
	}

	private static int GetMemberPosition(in SyntaxList<AttributeSyntax> attributes, in TokenList modifiers, in Token token)
	{
		if(!attributes.IsDefaultOrEmpty)
		{
			return attributes.Position;
		}

		if (!modifiers.IsDefaultOrEmpty)
		{
			return modifiers.Position;
		}

		return token.Position;
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
			SyntaxKind.ElementAccessExpression or
			SyntaxKind.PointerMemberAccessExpression
				=> Precedence.Primary,

			SyntaxKind.AddressOfExpression
				=> Precedence.AddressOf,

			SyntaxKind.PointerIndirectionExpression
				=> Precedence.PointerIndirection,

			SyntaxKind.CastExpression
				=> Precedence.Cast,

			SyntaxKind.UnaryPlusExpression or
			SyntaxKind.UnaryMinusExpression or
			SyntaxKind.BitwiseNotExpression or
			SyntaxKind.LogicalNotExpression or
			SyntaxKind.PreIncrementExpression or
			SyntaxKind.PreDecrementExpression or
			SyntaxKind.AddressOfExpression or
			SyntaxKind.PointerIndirectionExpression
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

	private static TextSpan GetSpan(int start, Token end)
	{
		int endPosition = end.Position + end.Length;
		int length = endPosition - start;
		return new(start, length);
	}

	private static TextSpan GetSpan(in Token start, in Token end)
	{
		return GetSpan(start.Position, end);
	}

	private static TextSpan GetSpan(int start, int end)
	{
		int length = end - start + 1;
		return new(start, length);
	}

	private static TextSpan GetSpan(int start, SyntaxNode end)
	{
		return GetSpan(start, end.Span.End);
	}


	private static TextSpan GetSpan(Token start, SyntaxNode end)
	{
		return GetSpan(start.Position, end.Span.End);
	}

	private static TextSpan GetSpan(SyntaxNode start, Token end)
	{
		return GetSpan(start.Position, end.Span.End);
	}

	private static TextSpan GetSpan(SyntaxNode start, SyntaxNode end)
	{
		return GetSpan(start.Span.Start, end.Span.End);
	}

	private static TextSpan GetSpan<TNode>(Token start, SyntaxList<TNode> end) where TNode : SyntaxNode
	{
		return end.IsDefaultOrEmpty
			? start.Span
			: GetSpan(start, end[^1]);
	}

	private static TextSpan GetSpan<TNode>(SyntaxNode start, SyntaxList<TNode> end) where TNode : SyntaxNode
	{
		return end.IsDefaultOrEmpty
			? start.Span
			: GetSpan(start, end[^1]);
	}

	private static TextSpan GetSpan<TNode>(Token start, List<TNode>? end) where TNode : SyntaxNode
	{
		return end is null || end.Count == 0
			? start.Span
			: GetSpan(start, end[^1]);
	}

	private static TextSpan GetSpan<TNode>(SyntaxNode start, List<TNode>? end) where TNode : SyntaxNode
	{
		return end is null || end.Count == 0
			? start.Span
			: GetSpan(start, end[^1]);
	}

	private static TextSpan GetSpan<TNode>(Token start, List<(TNode node, Token)>? end) where TNode : SyntaxNode
	{
		return end is null || end.Count == 0
			? start.Span
			: GetSpan(start, end[^1].node);
	}

	private static TextSpan GetSpan<TNode>(SyntaxNode start, List<(TNode node, Token)>? end) where TNode : SyntaxNode
	{
		return end is null || end.Count == 0
			? start.Span
			: GetSpan(start, end[^1].node);
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

	private void EnsureKind(ref Token token, TokenKind kind)
	{
		if (token.Kind != kind)
		{
			token = MissingTokenWithError(token);
		}
	}

	private Token EatContextualKeyword()
	{
		Token token = EatToken();
		AcceptContextualKeyword(ref token);
		return token;
	}

	private Token EatContextualKeyword(TokenKind kind)
	{
		Token token = Peek();

		if (token.Kind != kind)
		{
			return MissingTokenWithError(token);
		}

		AcceptContextualKeyword(ref token);
		return token;
	}

	private bool EatToken(TokenKind kind, out Token token)
	{
		ref readonly Token current = ref Peek();

		if (current.Kind != kind)
		{
			token = MissingTokenWithError(current);
			return false;
		}

		EatToken();
		token = current;
		return true;
	}

	private Token EatToken(TokenKind kind)
	{
		ref readonly Token token = ref Peek();

		if (token.Kind != kind)
		{
			return MissingTokenWithError(token);
		}

		EatToken();

		return token;
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

	private bool PeekKind(TokenKind kind, int pos)
	{
		ref readonly Token token = ref Peek(pos);
		return token.Kind == kind;
	}

	private static void AcceptContextualKeyword(ref Token token)
	{
		TokenKind kind = token.ContextualKind;

		if(kind == token.Kind)
		{
			return;
		}

		ChangeKind(ref token, kind);
	}

	private Token AcceptContextualKeyword(TokenKind kind)
	{
		Token token = Peek();
		ChangeKind(ref token, kind);
		return token;
	}

	private static void ChangeKind(ref Token token, TokenKind kind)
	{
		token = new(kind, token.Text, token.Position, token.Value);
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

	private Token SkippedToken()
	{
		return SkippedToken(Peek());
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

		// *a
		PointerIndirection,

		// &a
		AddressOf,

		// a.b, a++, etc.
		Primary
	}
}
