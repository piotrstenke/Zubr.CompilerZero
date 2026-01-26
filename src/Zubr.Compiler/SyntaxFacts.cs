namespace Zubr.Compiler;

public static class SyntaxFacts
{
	private const SyntaxKind START_KIND = SyntaxKind.None;
	private const SyntaxKind END_KIND = SyntaxKind.EOF;

	private const SyntaxKind KEYWORDS_START = SyntaxKind.UseKeyword;
	private const SyntaxKind KEYWORDS_END = SyntaxKind.StringKeyword;

	public static bool IsKeyword(string value)
	{
		return GetKeywordKind(value) != SyntaxKind.None;
	}

	public static bool IsKeyword(this in SyntaxToken token)
	{
		return IsKeyword(token.Kind);
	}

	public static bool IsKeyword(SyntaxKind value)
	{
		return IsBetween(value, KEYWORDS_START, KEYWORDS_END);
	}

	public static bool IsLiteralExpression(this in SyntaxToken token)
	{
		return IsLiteralExpression(token.Kind);
	}

	public static bool IsLiteralExpression(SyntaxKind value)
	{
		return
			value == SyntaxKind.TrueKeyword ||
			value == SyntaxKind.FalseKeyword ||
			value == SyntaxKind.StringLiteralToken ||
			value == SyntaxKind.CharLiteralToken ||
			value == SyntaxKind.NumericLiteralExpression;
	}

	public static bool IsPredefinedType(this in SyntaxToken token)
	{
		return IsPredefinedType(token.Kind);
	}

	public static bool IsPredefinedType(SyntaxKind value)
	{
		return
			value == SyntaxKind.BoolKeyword ||
			value == SyntaxKind.VoidKeyword ||
			value == SyntaxKind.IntKeyword ||
			value == SyntaxKind.StringKeyword ||
			value == SyntaxKind.CharKeyword;
	}

	public static bool IsTypeDeclarationKeyword(this in SyntaxToken token)
	{
		return IsTypeDeclarationKeyword(token.Kind);
	}

	public static bool IsTypeDeclarationKeyword(SyntaxKind value)
	{
		return
			value == SyntaxKind.ClassKeyword ||
			value == SyntaxKind.StructKeyword ||
			value == SyntaxKind.EnumKeyword ||
			value == SyntaxKind.TraitKeyword;
	}

	public static bool IsAssignmentOperator(this in SyntaxToken token)
	{
		return IsAssignmentOperator(token.Kind);
	}

	public static bool IsAssignmentOperator(SyntaxKind value)
	{
		return
			value == SyntaxKind.EqualsToken ||
			value == SyntaxKind.PlusEqualsToken ||
			value == SyntaxKind.MinusEqualsToken ||
			value == SyntaxKind.AsteriskEqualsToken ||
			value == SyntaxKind.SlashEqualsToken ||
			value == SyntaxKind.PercentEqualsToken ||
			value == SyntaxKind.BarEqualsToken ||
			value == SyntaxKind.CaretEqualsToken ||
			value == SyntaxKind.AmpersandEqualsToken ||
			value == SyntaxKind.LessThanLessThanEqualsToken ||
			value == SyntaxKind.GreaterThanGreaterThanEqualsToken ||
			value == SyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken;
	}

	public static bool IsAccessModifier(this in SyntaxToken token)
	{
		return IsAccessModifier(token.Kind);
	}

	public static bool IsAccessModifier(SyntaxKind value)
	{
		return
			value == SyntaxKind.PubKeyword ||
			value == SyntaxKind.ProtKeyword ||
			value == SyntaxKind.PrivKeyword ||
			value == SyntaxKind.ScopedKeyword;
	}

	public static bool IsModifier(this in SyntaxToken token)
	{
		return IsModifier(token.Kind);
	}

	public static bool IsModifier(SyntaxKind value)
	{
		return IsAccessModifier(value) || value == SyntaxKind.OpenKeyword;
	}

	public static bool IsValid(this in SyntaxToken token)
	{
		return IsValid(token.Kind);
	}

	public static bool IsValid(SyntaxKind value)
	{
		return IsBetween(value, START_KIND, END_KIND) && !IsError(value);
	}

	public static bool IsError(this in SyntaxToken token)
	{
		return IsError(token.Kind);
	}

	public static bool IsError(SyntaxKind value)
	{
		return value == SyntaxKind.BadToken || value == SyntaxKind.MissingToken;
	}

	public static SyntaxKind GetKeywordKind(string value)
	{
		return value switch
		{
			"pub" => SyntaxKind.PubKeyword,
			"prot" => SyntaxKind.ProtKeyword,
			"scoped" => SyntaxKind.ScopedKeyword,
			"priv" => SyntaxKind.PrivKeyword,
			"open" => SyntaxKind.OpenKeyword,
			"use" => SyntaxKind.UseKeyword,
			"as" => SyntaxKind.AsKeyword,
			"from" => SyntaxKind.FromKeyword,
			"module" => SyntaxKind.ModuleKeyword,
			"top" => SyntaxKind.TopKeyword,
			"global" => SyntaxKind.GlobalKeyword,
			"if" => SyntaxKind.IfKeyword,
			"elif" => SyntaxKind.ElifKeyword,
			"else" => SyntaxKind.ElseKeyword,
			"void" => SyntaxKind.VoidKeyword,
			"class" => SyntaxKind.ClassKeyword,
			"struct" => SyntaxKind.StructKeyword,
			"enum" => SyntaxKind.EnumKeyword,
			"trait" => SyntaxKind.TraitKeyword,
			"mut" => SyntaxKind.MutKeyword,
			"self" => SyntaxKind.SelfKeyword,
			"match" => SyntaxKind.MatchKeyword,
			"for" => SyntaxKind.ForKeyword,
			"do" => SyntaxKind.DoKeyword,
			"while" => SyntaxKind.WhileKeyword,
			"break" => SyntaxKind.BreakKeyword,
			"next" => SyntaxKind.NextKeyword,
			"return" => SyntaxKind.ReturnKeyword,
			"const" => SyntaxKind.ConstKeyword,
			"let" => SyntaxKind.LetKeyword,
			"bool" => SyntaxKind.BoolKeyword,
			"true" => SyntaxKind.TrueKeyword,
			"false" => SyntaxKind.FalseKeyword,
			"int" => SyntaxKind.IntKeyword,
			"string" => SyntaxKind.StringKeyword,
			"char" => SyntaxKind.CharKeyword,
			"give" => SyntaxKind.GiveKeyword,
			"to" => SyntaxKind.ToKeyword,
			"where" => SyntaxKind.WhereKeyword,
			_ => SyntaxKind.None
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

	private static bool IsBetween(SyntaxKind value, SyntaxKind start, SyntaxKind end)
	{
		return value >= start && value <= end;
	}
}
