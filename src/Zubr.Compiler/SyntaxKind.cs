namespace Zubr.Compiler;

public enum SyntaxKind : uint
{
	None = 0,

	CompilationUnit = 1,

	// -----------------------------
	// Tokens
	// -----------------------------

	// !
	ExclamationToken,

	// =
	EqualsToken,

	// ==
	EqualsEqualsToken,

	// >
	GreaterThanToken,

	// <
	LessThanToken,

	// >=
	GreaterThanOrEqualToken,

	// <=
	LessThanOrEqualToken,

	// +
	PlusToken,

	// ++
	PlusPlusToken,

	// +=
	PlusEqualsToken,

	// -
	MinusToken,

	// --
	MinusMinusToken,

	// -=
	MinusEqualsToken,

	// *
	AsteriskToken,

	// *=
	AsteriskEqualsToken,

	// %
	PercentToken,

	// %=
	PercentEqualsToken,

	// ^
	CaretToken,

	// ^=
	CaretEqualsToken,

	// |
	BarToken,

	// ||
	BarBarToken,

	// |=
	BarEqualsToken,

	// /
	SlashToken,

	// /=
	SlashEqualsToken,

	// &
	AmpersandToken,

	// &&
	AmpersandAmpersandToken,

	// &=
	AmpersandEqualsToken,

	// (
	OpenParenToken,

	// )
	CloseParentToken,

	// [
	OpenBracketToken,

	// ]
	CloseBracketToken,

	// {
	OpenBraceToken,

	// }
	CloseBraceToken,

	// :
	ColonToken,

	// ::
	ColonColonToken,

	// ;
	SemicolonToken,

	// ,
	CommaToken,

	// .
	DotToken,

	// ..
	DotDotToken,

	// ?
	QuestionToken,

	// Name of a type, method etc.
	IdentifierToken,

	// -----------------------------
	// Literals
	// -----------------------------

	// "some text"
	StringLiteral,

	// 1, 2L, 3u etc. 1.2f, 0.42e2 etc.
	NumericLiteral,

	// 'a'
	CharLiteral,

	// -----------------------------
	// Keywords
	// -----------------------------

	// use
	UseKeyword,

	// as
	AsKeyword,

	// from
	FromKeyword,

	// module
	ModuleKeyword,

	// top
	TopKeyword,

	// global
	GlobalKeyword,

	// class
	ClassKeyword,

	// struct
	StructKeyword,
	
	// self
	SelfKeyword,

	// enum
	EnumKeyword,

	// if
	IfKeyword,

	// elif
	ElifKeyword,

	// else
	ElseKeyword,

	// pub
	PubKeyword,

	// void
	VoidKeyword,

	// -----------------------------
	// Declarations
	// -----------------------------

	ModuleDeclaration,

	ClassDeclaration,

	// -----------------------------
	// Expressions
	// -----------------------------

	// -----------------------------
	// Statements & Directives
	// -----------------------------

	UseDirective,

	// -----------------------------
	// Names
	// -----------------------------

	IdentifierName,

	QualifiedName,

	GenericName,

	// -----------------------------
	// Errors
	// -----------------------------

	MissingToken,

	BadToken,

	// -----------------------------
	// Other
	// -----------------------------

	EOF = 9999
}
