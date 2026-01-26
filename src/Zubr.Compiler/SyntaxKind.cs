namespace Zubr.Compiler;

public enum SyntaxKind : uint
{
	None = 0,

	CompilationUnit = 1,

	// -----------------------------
	// Tokens
	// -----------------------------

	// =
	EqualsToken,

	// ==
	EqualsEqualsToken,

	// !
	ExclamationToken,

	// !=
	ExclamationEqualsToken,

	// >
	GreaterThanToken,

	// >>
	GreaterThanGreaterThanToken,

	// >>>
	GreaterThanGreaterThanGreaterThanToken,

	// >=
	GreaterThanEqualsToken,

	// >>=
	GreaterThanGreaterThanEqualsToken,

	// >>>=
	GreaterThanGreaterThanGreaterThanEqualsToken,

	// <
	LessThanToken,

	// <<
	LessThanLessThanToken,

	// <=
	LessThanEqualsToken,

	// <<=
	LessThanLessThanEqualsToken,

	// <<<=
	LessThanLessThanLessThanEqualsToken,

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

	// ~
	TildeToken,

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

	// char
	CharKeyword,

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

	// a + b
	AddExpression,

	// a - b
	SubtractExpression,

	// a * b
	MultiplyExpression,

	// a / b
	DivideExpression,

	// a % b
	ModuloExpression,

	// a << b
	LeftShiftExpression,

	// a >> b
	RightShiftExpression,

	// a >>> b
	UnsignedRightShiftExpression,

	// a | b
	BitwiseOrExpression,

	// a & b
	BitwiseAndExpression,

	// a ^ b
	ExclusiveOrExpression,

	// a || b
	LogicalOrExpression,

	// a && b
	LogicalAndExpression,

	// a == b
	EqualsExpression,

	// a != b
	NotEqualsExpression,

	// a < b
	LessThanExpression,

	// a <=
	LessThanOrEqualExpression,

	// a > b
	GreaterThanExpression,

	// a >= b
	GreaterThanOrEqualExpression,

	// +a
	UnaryPlusExpression,

	// -a
	UnaryMinusExpression,

	// ~a
	BitwiseNotExpression,

	// !a
	LogicalNotExpression,

	// ++a
	PreIncrementExpression,

	// --a
	PreDecrementExpression,

	// a++
	PostIncrementExpression,

	// a--
	PostDecrementExpression,

	// a = b
	AssignmentExpression,

	// a += b
	AddAssignmentExpression,

	// a -= b
	SubtractAssignmentExpression,

	// a *= b
	MultiplyAssignmentExpression,

	// a /= b
	DivideAssignmentExpression,

	// a %= b
	ModuloAssignmentExpression,

	// a <<= b
	LeftShiftAssignmentExpression,

	// a >>= b
	RightShiftAssignmentExpression,

	// a >>>= b
	UnsignedRightShiftAssignmentExpression,

	// a |= b
	OrAssignmentExpression,

	// a &= b
	AndAssignmentExpression,

	// a ^= b
	ExclusiveOrAssignmentExpression,

	// (a + b)
	ParenthesizedExpression,

	// a ? b : c
	ConditionalExpression,

	// (int)a
	CastExpression,

	// self
	SelfExpression,

	// a.b
	MemberAccessExpression,

	// a(1, 2)
	InvocationExpression,

	// the '(1, 2)' in 'a(1, 2)'
	ArgumentList,

	// the '1' and '2 'in 'a(1, 2)'
	Argument,

	// -----------------------------
	// Names
	// -----------------------------

	// Test
	IdentifierName,

	// Module.Test
	QualifiedName,

	// Test<T>
	GenericName,

	// '<T>' in 'Test<T>'
	TypeArgumentList,

	// int, string, bool etc.
	PredefinedType,

	// -----------------------------
	// Statements
	// -----------------------------

	// { }
	Block,

	// ;
	EmptyStatement,

	// Expression followed by a ';'
	ExpressionStatement,

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
