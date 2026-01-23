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
	CloseParenToken,

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
	StringLiteralToken,

	// 1, 2L, 3u etc. 1.2f, 0.42e2 etc.
	NumericLiteralToken,

	// 'a'
	CharLiteralToken,

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

	// match
	MatchKeyword,

	// for
	ForKeyword,

	// do
	DoKeyword,

	// while
	WhileKeyword,

	// break
	BreakKeyword,

	// next
	NextKeyword,

	// return
	ReturnKeyword,

	// pub
	PubKeyword,

	// prot
	ProtKeyword,

	// scoped
	ScopedKeyword,

	// priv
	PrivKeyword,

	// open
	OpenKeyword,

	// void
	VoidKeyword,

	// mut
	MutKeyword,

	// const
	ConstKeyword,

	// let
	LetKeyword,

	// bool
	BoolKeyword,

	// true
	TrueKeyword,

	// false
	FalseKeyword,

	// int
	IntKeyword,

	// string
	StringKeyword,

	// -----------------------------
	// Declarations
	// -----------------------------

	ModuleDeclaration,

	ClassDeclaration,

	StructDeclaration,

	FunctionDeclaration,

	ParameterList,

	Parameter,

	TypeParameterList,

	TypeParameter,

	// -----------------------------
	// Expressions
	// -----------------------------

	// "some text"
	StringLiteralExpression,

	// 1, 2L, 3u etc. 1.2f, 0.42e2 etc.
	NumericLiteralExpression,

	// 'a'
	CharLiteralExpression,

	// true
	TrueLiteralExpression,

	// false
	FalseLiteralExpression,

	// -----------------------------
	// Statements & Directives
	// -----------------------------

	Block,

	UseDirective,

	// -----------------------------
	// Clauses
	// -----------------------------

	EqualsValue,

	// -----------------------------
	// Names
	// -----------------------------

	IdentifierName,

	QualifiedName,

	GenericName,

	PredefinedType,

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
