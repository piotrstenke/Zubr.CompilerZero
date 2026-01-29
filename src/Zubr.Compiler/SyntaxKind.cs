namespace Zubr.Compiler;

public enum SyntaxKind : uint
{
	// Unknown syntax kind.
	None = 0,

	// Root of the syntax tree.
	CompilationUnit = 1,

	// -----------------------------
	// Directives
	// -----------------------------

	// use Module as M;
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

	// void main() { }
	FunctionDeclaration,

	// the '(int a, bool b)' in 'foo(int a, bool b)'
	ParameterList,

	// the 'int a' and 'bool b' in 'foo(int a, bool b)'
	Parameter,

	// the '<int, string>' in 'Test<int, string>'
	TypeParameterList,

	// the 'int' and 'string' in 'Test<int, string>'
	TypeParameter,

	// where T : X, class, U : T, struct
	TypeParameterConstraintList,

	// T : X, class
	TypeParameterConstraintClause,

	// the 'class' in 'where T : class'
	ClassConstraint,

	// the 'struct' in 'where T : struct'
	StructConstraint,

	// the 'X' in 'where T : X'
	TypeConstraint,

	// -----------------------------
	// Expressions
	// -----------------------------

	// "some text"
	StringLiteralExpression,

	// 1, 2L, 3u, 1.2f, 0.42e2 etc.
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

	// int x = 1;
	LocalDeclarationStatement,

	// do { } while (condition);
	DoStatement,

	// while(condition) { }
	WhileStatement,

	// for (x : collection) { }
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
}
