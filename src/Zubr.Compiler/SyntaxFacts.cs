namespace Zubr.Compiler;

public static class SyntaxFacts
{
	private const TokenKind START_KIND = TokenKind.None;
	private const TokenKind END_KIND = TokenKind.EOF;

	private const TokenKind KEYWORDS_START = TokenKind.UseKeyword;
	private const TokenKind KEYWORDS_END = TokenKind.StringKeyword;

	public static bool IsKeyword(string value)
	{
		return GetKeywordKind(value) != TokenKind.None;
	}

	public static bool IsKeyword(this in Token token)
	{
		return IsKeyword(token.Kind);
	}

	public static bool IsKeyword(TokenKind value)
	{
		return IsBetween(value, KEYWORDS_START, KEYWORDS_END);
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
		return
			value == TokenKind.BoolKeyword ||
			value == TokenKind.VoidKeyword ||
			value == TokenKind.IntKeyword ||
			value == TokenKind.StringKeyword ||
			value == TokenKind.CharKeyword ||
			value == TokenKind.AnyKeyword;
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

	public static bool IsAccessor(this in Token token)
	{
		return IsAccessor(token.Kind);
	}

	public static bool IsAccessor(TokenKind value)
	{
		return value is
			TokenKind.GetKeyword or
			TokenKind.SetKeyword;
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
			value == TokenKind.ScopedKeyword;
	}

	public static bool IsModifier(this in Token token)
	{
		return IsModifier(token.Kind);
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
			TokenKind.ReqKeyword;
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

	public static TokenKind GetKeywordKind(string value)
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
