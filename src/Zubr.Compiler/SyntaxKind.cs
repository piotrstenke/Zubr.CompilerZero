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

	// trait
	TraitKeyword,
	
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

	// stop
	StopKeyword,

	// next
	NextKeyword,

	// return
	ReturnKeyword,

	// give
	GiveKeyword,

	// to
	ToKeyword,

	// type
	TypeKeyword,

	// where
	WhereKeyword,

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

	// extern
	ExternKeyword,

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
	// Directives
	// -----------------------------

	UseDirective,

	// -----------------------------
	// Declarations
	// -----------------------------

	// module Test;
	ModuleDeclaration,

	// class Test { }
	ClassDeclaration,

	// struct Test { }
	StructDeclaration,

	// void Main() { }
	FunctionDeclaration,

	ParameterList,

	Parameter,

	TypeParameterList,

	TypeParameter,

	TypeParameterConstraintList,

	TypeParameterConstraintClause,

	ClassConstraint,

	StructConstraint,

	TypeConstraint,

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

	// int a
	VariableExpression,

	// -----------------------------
	// Names
	// -----------------------------

	// Test
	IdentifierName,

	// Module.Test
	QualifiedName,

	// Test<T>
	GenericName,

	// int, string, bool etc.
	PredefinedType,

	// -----------------------------
	// Statements
	// -----------------------------

	// { }
	Block,

	// ;
	EmptyStatement,

	// return x;
	ReturnStatement,

	// stop;
	StopStatement,

	// next;
	NextStatement,

	/// int x = 1;
	LocalDeclarationStatement,

	// do { } while (condition);
	DoStatement,

	// while(condition) { }
	WhileStatement,

	// for (index, x : collection) { }
	ForStatement,

	// if (x == y) { }
	IfStatement,

	// elif (x == y) { }
	ElifClause,

	// else { }
	ElseClause,

	// int x = 1;
	VariableDeclaration,

	// x = 1;
	VariableDeclarator,

	// -----------------------------
	// Clauses
	// -----------------------------

	// = value
	EqualsValue,

	// -----------------------------
	// Errors
	// -----------------------------

	MissingToken,

	BadToken,

	// -----------------------------
	// Other
	// -----------------------------

	// End of file
	EOF = 9999
}
