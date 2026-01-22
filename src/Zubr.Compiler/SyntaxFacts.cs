namespace Zubr.Compiler;

public static class SyntaxFacts
{
	private const SyntaxKind START_KIND = SyntaxKind.None;
	private const SyntaxKind END_KIND = SyntaxKind.EOF;

	private const SyntaxKind KEYWORDS_START = SyntaxKind.UseKeyword;
	private const SyntaxKind KEYWORDS_END = SyntaxKind.VoidKeyword;

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

	public static bool IsValid(this SyntaxToken token)
	{
		return IsValid(token.Kind);
	}

	public static bool IsValid(SyntaxKind value)
	{
		return IsBetween(value, START_KIND, END_KIND) && !IsError(value);
	}

	public static bool IsError(this SyntaxToken token)
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
