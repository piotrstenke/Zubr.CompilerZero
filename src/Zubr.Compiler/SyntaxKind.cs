namespace Zubr.Compiler;

public enum SyntaxKind : uint
{
	// Unknown syntax kind.
	None = 0,

	// Root of the syntax tree.
	CompilationUnit = 1,

	// -----------------------------
	// Directives & Attributes
	// -----------------------------

	// use Module as M;
	UseDirective,

	// pub alias Test = LongTestName;
	AliasDirective,

	// [assembly: Value(2)]
	Attribute,

	// the 'assembly' in '[assembly: Value(2)]'
	AttributeTarget,

	// the '(2)' in '[assembly: Value(2)]'
	AttributeArgumentList,

	// the '2' in '[assembly: Value(2)]'
	AttributeArgument,

	// -----------------------------
	// Declarations
	// -----------------------------

	// module Test;
	ModuleDeclaration,

	// class Test { }
	ClassDeclaration,

	// struct Test { }
	StructDeclaration,

	// trait Test { }
	TraitDeclaration,

	// impl Test<T> { }, impl Test<T> : Stringable { }
	ImplementationDeclaration,

	// union Result<T> = T | Error
	UnionDeclaration,

	// enum Test { A, B, C }
	EnumDeclaration,

	// enum struct Test(int value) { A(5), A(3), C }
	EnumStructDeclaration,

	// enum class Test(int value) { A(5) { ... }, A(3), C }
	EnumClassDeclaration,

	// attr Test { }
	AttributeDeclaration,

	// void main() { }
	FunctionDeclaration,

	// new(int a) { }
	ConstructorDeclaration,

	// free() { } or gcfree() { }
	DestructorDeclaration,

	// pub int a = 1;, pub int a => 1;, pub int a { get => 1; } etc.
	PropertyDeclaration,

	// field int a = 2;
	FieldDeclaration,

	// { get => 1; priv set => field = value }
	AccessorList,

	// get; get => 1; get { } etc.
	AccessorDeclaration,

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

	// the ': X, Y' in 'class Type : X, Y'
	BaseTypeList,

	// the 'X' and 'Y' in 'class Type : X, Y'
	SimpleBaseType,

	// the 'X(a, b)' om 'class Type(int a, int b) : X(a, b)'
	PrimaryBaseType,

	// A, B, C in 'enum Test { A, B, C }'
	SimpleEnumMemberDeclaration,

	// A(2), B(4) in 'enum struct Test { A(2), B(4) }'
	ComplexEnumMemberDeclaration,

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

	// base
	BaseExpression,

	// new { ... }, new(), new Test() { } etc.
	ObjectCreationExpression,

	// new int[] { }, new[5] etc.
	ArrayCreationExpression,

	// placeholder for expression when array type has no specified size (e.g. int[])
	SkippedArraySizeExpression,

	// { "text", 1, new() { } }
	InitializerExpression,

	// 1..array.length
	RangeExpression,

	// [], [1, 2, 3], [1, a.., 2] etc.
	CollectionExpression,

	// ..array
	SpreadExpression,

	// a.b
	MemberAccessExpression,

	// a(1, 2)
	InvocationExpression,

	// base(1, 2)
	BaseConstructorInvocationExpression,

	// self(1, 2)
	SelfConstructorInvocationExpression,

	// the '(1, 2)' in 'a(1, 2)'
	ArgumentList,

	// the '1' and '2 'in 'a(1, 2)'
	Argument,

	// -----------------------------
	// Types & Names
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

	// int?
	NullableType,

	// float[], int[,] etc.
	ArrayType,

	// the '[,]' in 'int[,]' 
	ArrayRank,

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

	// x = 1
	VariableDeclaration,

	// int main() { int test() { } }
	LocalFunctionStatement,

	// -----------------------------
	// Clauses
	// -----------------------------

	// = value
	EqualsValueClause,

	// => 1 + 1
	ArrowExpressionClause,

	// Value = 
	NameEqualsClause
}
