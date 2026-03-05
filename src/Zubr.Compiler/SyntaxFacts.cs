using System;

namespace Zubr.Compiler;

public static class SyntaxFacts
{
	private const TokenKind START_KIND = TokenKind.None;
	private const TokenKind END_KIND = TokenKind.EOF;

	private const TokenKind KEYWORDS_START = TokenKind.UseKeyword;
	private const TokenKind KEYWORDS_END = TokenKind.StringKeyword;

	public static bool IsKeyword(string value)
	{
		return GetKeyword(value) != TokenKind.None;
	}

	public static bool IsKeyword(this in Token token)
	{
		return IsKeyword(token.Kind);
	}

	public static bool IsKeyword(TokenKind value)
	{
		return IsBetween(value, KEYWORDS_START, KEYWORDS_END);
	}

	public static TokenKind GetContextualKind(this in Token token)
	{
		if (!token.IsKind(TokenKind.IdentifierToken))
		{
			return token.Kind;
		}

		TokenKind kind = GetKeyword(token.Text);

		if(kind == default)
		{
			return token.Kind;
		}

		return kind;
	}

	public static bool IsContextualKeyword(this in Token token)
	{
		if(!token.IsKind(TokenKind.IdentifierToken))
		{
			return false;
		}

		return GetKeyword(token.Text) != TokenKind.None;
	}

	public static bool IsContextualKeyword(TokenKind value)
	{
		return
			value == TokenKind.FreeKeyword ||
			value == TokenKind.GetKeyword ||
			value == TokenKind.SetKeyword ||
			value == TokenKind.InitKeyword ||
			value == TokenKind.ValueKeyword ||
			value == TokenKind.StopKeyword ||
			value == TokenKind.NextKeyword ||
			value == TokenKind.FileKeyword ||
			value == TokenKind.AssemblyKeyword ||
			value == TokenKind.OverKeyword ||
			value == TokenKind.FlagKeyword ||
			value == TokenKind.OpenKeyword ||
			value == TokenKind.LimitKeyword ||
			value == TokenKind.ScopedKeyword ||
			value == TokenKind.FinalKeyword ||
			value == TokenKind.ManagedKeyword ||
			value == TokenKind.UnmanagedKeyword ||
			value == TokenKind.LocalKeyword;
	}

	public static void A<T>(int length) where T : unmanaged
	{
		Span<T> s = stackalloc T[length];
	}

	public static bool IsLiteralExpression(SyntaxKind value)
	{
		return
			value == SyntaxKind.TrueLiteralExpression ||
			value == SyntaxKind.FalseLiteralExpression ||
			value == SyntaxKind.StringLiteralExpression ||
			value == SyntaxKind.CharLiteralExpression ||
			value == SyntaxKind.NumericLiteralExpression;
	}

	public static bool IsPredefinedType(this in Token token)
	{
		return IsPredefinedType(token.Kind);
	}

	public static bool IsPredefinedType(TokenKind value)
	{
		return value is
			TokenKind.VoidKeyword or
			TokenKind.BoolKeyword or
			TokenKind.IntKeyword or
			TokenKind.UIntKeyword or
			TokenKind.ShortKeyword or
			TokenKind.UShortKeyword or
			TokenKind.LongKeyword or
			TokenKind.ULongKeyword or
			TokenKind.ByteKeyword or
			TokenKind.SByteKeyword or
			TokenKind.StringKeyword or
			TokenKind.CharKeyword or
			TokenKind.FloatKeyword or
			TokenKind.DoubleKeyword or
			TokenKind.HalfKeyword or
			TokenKind.DecimalKeyword or
			TokenKind.NIntKeyword or
			TokenKind.NUIntKeyword or
			TokenKind.AnyKeyword;
	}

	public static bool IsTypeDeclarationKeyword(this in Token token)
	{
		return IsTypeDeclarationKeyword(token.Kind);
	}

	public static bool IsTypeDeclarationKeyword(TokenKind value)
	{
		return value is
			TokenKind.ClassKeyword or
			TokenKind.StructKeyword or
			TokenKind.EnumKeyword or
			TokenKind.TraitKeyword or
			TokenKind.AttrKeyword;
	}

	public static SyntaxKind GetPrefixUnaryExpressionKind(TokenKind value)
	{
		return value switch
		{
			TokenKind.PlusToken => SyntaxKind.UnaryPlusExpression,
			TokenKind.PlusPlusToken => SyntaxKind.PreIncrementExpression,
			TokenKind.MinusToken => SyntaxKind.UnaryMinusExpression,
			TokenKind.MinusMinusToken => SyntaxKind.PreDecrementExpression,
			TokenKind.ExclamationToken => SyntaxKind.LogicalNotExpression,
			TokenKind.TildeToken => SyntaxKind.BitwiseNotExpression,
			TokenKind.AmpersandToken => SyntaxKind.AddressOfExpression,
			TokenKind.AsteriskToken => SyntaxKind.PointerIndirectionExpression,
			_ => default
		};
	}

	public static SyntaxKind GetPostfixUnaryExpressionKind(TokenKind value)
	{
		return value switch
		{
			TokenKind.PlusPlusToken => SyntaxKind.PostIncrementExpression,
			TokenKind.MinusMinusToken => SyntaxKind.PostDecrementExpression,
			_ => default
		};
	}

	public static SyntaxKind GetBinaryExpressionKind(TokenKind value)
	{
		return value switch
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
			TokenKind.EqualsEqualsEqualsToken => SyntaxKind.ReferenceEqualsExpression,
			TokenKind.ExclamationEqualsToken => SyntaxKind.NotEqualsExpression,
			TokenKind.ExclamationEqualsEqualsToken => SyntaxKind.ReferenceNotEqualsExpression,
			TokenKind.GreaterThanToken => SyntaxKind.GreaterThanExpression,
			TokenKind.GreaterThanEqualsToken => SyntaxKind.GreaterThanOrEqualExpression,
			TokenKind.LessThanToken => SyntaxKind.LessThanExpression,
			TokenKind.LessThanEqualsToken => SyntaxKind.LessThanOrEqualExpression,
			TokenKind.BarBarToken => SyntaxKind.LogicalOrExpression,
			TokenKind.AmpersandAmpersandToken => SyntaxKind.LogicalAndExpression,
			TokenKind.DotDotToken => SyntaxKind.RangeExpression,

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

	public static SyntaxKind GetLiteralExpressionKind(TokenKind value)
	{
		return value switch
		{
			TokenKind.StringLiteralToken => SyntaxKind.StringLiteralExpression,
			TokenKind.NumericLiteralToken => SyntaxKind.NumericLiteralExpression,
			TokenKind.CharLiteralToken => SyntaxKind.CharLiteralExpression,
			TokenKind.TrueKeyword => SyntaxKind.TrueLiteralExpression,
			TokenKind.FalseKeyword => SyntaxKind.FalseLiteralExpression,
			_ => default
		};
	}

	public static SyntaxKind GetExpressionKind(TokenKind value)
	{
		SyntaxKind kind = GetBinaryExpressionKind(value);

		if(kind != default)
		{
			return kind;
		}

		kind = GetPostfixUnaryExpressionKind(value);

		if(kind != default)
		{
			return kind;
		}

		kind = GetPrefixUnaryExpressionKind(value);

		if(kind != default)
		{
			return kind;
		}

		kind = GetLiteralExpressionKind(value);

		return kind;
	}

	public static bool IsOverloadableOperator(this Token token)
	{
		return IsOverloadableOperator(token.Kind);
	}

	public static bool IsOverloadableOperator(TokenKind value)
	{
		if(IsComparisonOperator(value))
		{
			return true;
		}

		if(GetPrefixUnaryExpressionKind(value) != default)
		{
			return true;
		}

		return value is
			TokenKind.AsteriskToken or
			TokenKind.PercentToken or
			TokenKind.SlashToken or
			TokenKind.AmpersandToken or
			TokenKind.BarToken or
			TokenKind.CaretToken or
			TokenKind.LessThanLessThanToken or
			TokenKind.GreaterThanGreaterThanToken or
			TokenKind.GreaterThanGreaterThanGreaterThanToken or
			TokenKind.FalseKeyword or
			TokenKind.TrueKeyword;
	}

	public static bool IsComparisonOperator(this in Token token)
	{
		return IsComparisonOperator(token.Kind);
	}

	public static bool IsComparisonOperator(TokenKind value)
	{
		return value is
			TokenKind.EqualsEqualsToken or
			TokenKind.EqualsEqualsEqualsToken or
			TokenKind.ExclamationEqualsToken or
			TokenKind.ExclamationEqualsEqualsToken or
			TokenKind.GreaterThanToken or
			TokenKind.GreaterThanEqualsToken or
			TokenKind.LessThanToken or
			TokenKind.LessThanEqualsToken;
	}

	public static bool IsAssignmentOperator(this in Token token)
	{
		return IsAssignmentOperator(token.Kind);
	}

	public static bool IsAssignmentOperator(TokenKind value)
	{
		return value is
			TokenKind.EqualsToken or
			TokenKind.PlusEqualsToken or
			TokenKind.MinusEqualsToken or
			TokenKind.AsteriskEqualsToken or
			TokenKind.SlashEqualsToken or
			TokenKind.PercentEqualsToken or
			TokenKind.BarEqualsToken or
			TokenKind.CaretEqualsToken or
			TokenKind.AmpersandEqualsToken or
			TokenKind.LessThanLessThanEqualsToken or
			TokenKind.GreaterThanGreaterThanEqualsToken or
			TokenKind.GreaterThanGreaterThanGreaterThanEqualsToken;
	}

	public static SyntaxKind GetAccessorKind(this in Token token)
	{
		return GetAccessorKind(token.Kind);
	}

	public static SyntaxKind GetAccessorKind(TokenKind value)
	{
		return value switch
		{
			TokenKind.GetKeyword => SyntaxKind.GetAccessorDeclaration,
			TokenKind.SetKeyword => SyntaxKind.SetAccessorDeclaration,
			_ => default
		};
	}

	public static bool IsAccessor(this in Token token)
	{
		return IsAccessor(token.Kind);
	}

	public static bool IsAccessor(TokenKind value)
	{
		return GetAccessorKind(value) != default;
	}

	public static bool IsAccessModifier(this in Token token)
	{
		return IsAccessModifier(token.Kind);
	}

	public static bool IsAccessModifier(TokenKind value)
	{
		return
			value == TokenKind.PubKeyword ||
			value == TokenKind.ProtKeyword ||
			value == TokenKind.PrivKeyword ||
			value == TokenKind.ScopedKeyword ||
			value == TokenKind.FileKeyword;
	}

	public static bool IsModifier(this in Token token)
	{
		return IsModifier(token.ContextualKind);
	}

	public static bool IsModifier(TokenKind value)
	{
		if(IsAccessModifier(value))
		{
			return true;
		}

		return value is
			TokenKind.OpenKeyword or
			TokenKind.MutKeyword or
			TokenKind.InitKeyword or
			TokenKind.FinalKeyword or
			TokenKind.FlagKeyword or
			TokenKind.DataKeyword or
			TokenKind.ConstKeyword or
			TokenKind.BaseKeyword or
			TokenKind.LimitKeyword or
			TokenKind.OverKeyword or
			TokenKind.ReqKeyword or
			TokenKind.StatKeyword or
			TokenKind.AutoKeyword or
			TokenKind.ManagedKeyword or
			TokenKind.UnmanagedKeyword or
			TokenKind.UnsafeKeyword or
			TokenKind.LocalKeyword;
	}

	public static bool IsValid(this in Token token)
	{
		return IsValid(token.Kind);
	}

	public static bool IsValid(TokenKind value)
	{
		return IsBetween(value, START_KIND, END_KIND) && !IsError(value);
	}

	public static bool IsError(this in Token token)
	{
		return IsError(token.Kind);
	}

	public static bool IsError(TokenKind value)
	{
		return value == TokenKind.BadToken || value == TokenKind.MissingToken;
	}

	public static TokenKind GetKeyword(string value)
	{
		return value switch
		{
			"pub" => TokenKind.PubKeyword,
			"prot" => TokenKind.ProtKeyword,
			"scoped" => TokenKind.ScopedKeyword,
			"priv" => TokenKind.PrivKeyword,
			"open" => TokenKind.OpenKeyword,
			"use" => TokenKind.UseKeyword,
			"as" => TokenKind.AsKeyword,
			"from" => TokenKind.FromKeyword,
			"module" => TokenKind.ModuleKeyword,
			"top" => TokenKind.TopKeyword,
			"if" => TokenKind.IfKeyword,
			"elif" => TokenKind.ElifKeyword,
			"else" => TokenKind.ElseKeyword,
			"get" => TokenKind.GetKeyword,
			"set" => TokenKind.SetKeyword,
			"init" => TokenKind.InitKeyword,
			"void" => TokenKind.VoidKeyword,
			"class" => TokenKind.ClassKeyword,
			"struct" => TokenKind.StructKeyword,
			"enum" => TokenKind.EnumKeyword,
			"trait" => TokenKind.TraitKeyword,
			"attr" => TokenKind.AttrKeyword,
			"mut" => TokenKind.MutKeyword,
			"self" => TokenKind.SelfKeyword,
			"match" => TokenKind.MatchKeyword,
			"base" => TokenKind.BaseKeyword,
			"over" => TokenKind.OverKeyword,
			"new" => TokenKind.NewKeyword,
			"free" => TokenKind.FreeKeyword,
			"gcfree" => TokenKind.GCFreeKeyword,
			"for" => TokenKind.ForKeyword,
			"do" => TokenKind.DoKeyword,
			"while" => TokenKind.WhileKeyword,
			"stop" => TokenKind.StopKeyword,
			"next" => TokenKind.NextKeyword,
			"return" => TokenKind.ReturnKeyword,
			"const" => TokenKind.ConstKeyword,
			"let" => TokenKind.LetKeyword,
			"null" => TokenKind.NullKeyword,
			"bool" => TokenKind.BoolKeyword,
			"true" => TokenKind.TrueKeyword,
			"false" => TokenKind.FalseKeyword,
			"any" => TokenKind.AnyKeyword,
			"int" => TokenKind.IntKeyword,
			"short" => TokenKind.ShortKeyword,
			"long" => TokenKind.LongKeyword,
			"byte" => TokenKind.ByteKeyword,
			"uint" => TokenKind.UIntKeyword,
			"ushort" => TokenKind.UShortKeyword,
			"ulong" => TokenKind.ULongKeyword,
			"sbyte" => TokenKind.SByteKeyword,
			"nint" => TokenKind.NIntKeyword,
			"nuint" => TokenKind.NUIntKeyword,
			"half" => TokenKind.HalfKeyword,
			"float" => TokenKind.FloatKeyword,
			"double" => TokenKind.DoubleKeyword,
			"decimal" => TokenKind.DecimalKeyword,
			"string" => TokenKind.StringKeyword,
			"char" => TokenKind.CharKeyword,
			"where" => TokenKind.WhereKeyword,
			"and" => TokenKind.AndKeyword,
			"or" => TokenKind.OrKeyword,
			"not" => TokenKind.NotKeyword,
			"is" => TokenKind.IsKeyword,
			"file" => TokenKind.FileKeyword,
			"field" => TokenKind.FieldKeyword,
			"assembly" => TokenKind.AssemblyKeyword,
			"data" => TokenKind.DataKeyword,
			"flag" => TokenKind.FlagKeyword,
			"union" => TokenKind.UnionKeyword,
			"alias" => TokenKind.AliasKeyword,
			"final" => TokenKind.FinalKeyword,
			"req" => TokenKind.ReqKeyword,
			"stat" => TokenKind.StatKeyword,
			"cast" => TokenKind.CastKeyword,
			"oper" => TokenKind.OperKeyword,
			"goto" => TokenKind.GotoKeyword,
			"value" => TokenKind.ValueKeyword,
			"managed" => TokenKind.ManagedKeyword,
			"unmanaged" => TokenKind.UnmanagedKeyword,
			"unsafe" =>	TokenKind.UnsafeKeyword,
			"local" => TokenKind.LocalKeyword,
			_ => TokenKind.None
		};
	}

	public static bool IsWhiteSpace(char c)
	{
		return
			c == ' ' ||
			c == '\t' ||
			c == '\v' ||
			c == '\f';
	}

	public static bool IsNewLine(char c)
	{
		return
			c == '\r' ||
			c == '\n';
	}

	public static bool IsValidIdentifierCharacter(char c)
	{
		return c == '_' || IsDigit(c) || IsAsciiLetter(c);
	}

	public static bool IsDigit(char c)
	{
		return c >= '0' && c <= '9';
	}

	public static bool IsAsciiLetter(char c)
	{
		return IsLowerCaseAsciiLetter(c) || IsUpperCaseAsciiLetter(c);
	}

	public static bool IsLowerCaseAsciiLetter(char c)
	{
		return c >= 'a' && c <= 'z';
	}

	public static bool IsUpperCaseAsciiLetter(char c)
	{
		return c >= 'A' && c <= 'Z';
	}

	private static bool IsBetween(TokenKind value, TokenKind start, TokenKind end)
	{
		return value >= start && value <= end;
	}
}
