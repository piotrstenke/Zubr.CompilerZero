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

	// use Module.Inner;
	// use Module.Inner as I;
	SimpleUseDirective,

	// use { X, Y, Z as Z1 } from Module;
	ListedUseDirective,

	// the '{ X, Y, Z as Z1 }' in 'use { X, Y, Z as Z1 } from Module;'
	UseDirectiveElementList,

	// the 'X', 'Y', 'Z as Z1' in 'use { X, Y, Z as Z1 } from Module;'
	UseDirectiveElement,

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

	// pub int oper+(int a, int b) { } 
	OperatorDeclaration,

	// pub cast bool(int a)
	CastDeclaration,

	// pub int a = 1;, pub int a => 1;, pub int a { get => 1; } etc.
	PropertyDeclaration,

	// pub int self[int index] => 1, pub int [int index] { get => 2; } etc.
	IndexerDeclaration,

	// pub int self(int index) { ... }
	InvokerDeclaration,

	// field int a = 2;
	FieldDeclaration,

	// { get => 1; priv set => field = value }
	AccessorList,

	// get; get => 1; get { } etc.
	GetAccessorDeclaration,

	// set; set => field = value; set { } etc.
	SetAccessorDeclaration,

	// the '(int a, bool b)' in 'foo(int a, bool b)'
	ParameterList,

	// the 'int a' and 'bool b' in 'foo(int a, bool b)'
	Parameter,

	// the '[int a, int b]' in 'pub T [int a, int b]'
	BracketParameterList,

	// the '<int, string>' in 'Test<int, string>'
	TypeParameterList,

	// the 'int' and 'string' in 'Test<int, string>'
	TypeParameter,

	// the ': Stringable' in 'class A<T : Stringable>'
	TypeParameterInlineConstraint,

	// where T : X, class, U : T, struct
	TypeParameterConstraintList,

	// T : X, class
	TypeParameterConstraintClause,

	// the 'class' in 'where T : class'
	ClassConstraint,

	// the 'struct' in 'where T : struct'
	StructConstraint,

	// the 'enum' in 'where T : enum'
	EnumConstraint,

	// the 'unmanaged' in 'where T : unmanaged'
	UnmanagedConstraint,

	// the 'self' in 'where T : self'
	SelfConstraint,

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

	// null
	NullLiteralExpression,

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

	// a === b
	ReferenceEqualsExpression,

	// a !== b
	ReferenceNotEqualsExpression,

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

	// *a
	PointerIndirectionExpression,

	// &b
	AddressOfExpression,

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

	// array[0]
	ElementAccessExpression,

	// { "text", 1, new() { } }
	InitializerExpression,

	// 1..array.length
	RangeExpression,

	// [], [1, 2, 3], [1, a.., 2] etc.
	CollectionExpression,

	// ..array
	SpreadExpression,

	// a.b
	SimpleMemberAccessExpression,

	// a->b
	PointerMemberAccessExpression,

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

	// the '[0]' in 'array[0]'
	BracketArgumentList,

	// (a, b)
	TupleExpression,

	// the 'int a' in '(int a, int b)'
	DeclarationExpression,

	// -----------------------------
	// Types & Names
	// -----------------------------

	// Test
	IdentifierName,

	// Module.Test
	QualifiedName,

	// ::Module.Test
	TopQualifiedName,

	// Test<T>
	GenericName,

	// the empty in type argument in Test<>
	SkippedTypeArgument,

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

	// int*
	PointerType,

	// int&
	ReferenceType,

	// let
	LetType,

	// (int a, int b)
	TupleType,

	// the 'int a' in '(int a, int b)'
	TupleElement,

	// int | string
	UnionType,

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

	// goto DEFAULT;
	GotoStatement,

	// DEFAULT: ...;
	LabelStatement,

	// do { } while (condition);
	DoStatement,

	// while(condition) { }
	WhileStatement,

	// for (int i = 0; i < 10; i++) { }
	ForStatement,

	// for (x : collection) { }
	RangedForStatement,

	// if (x == y) { }
	IfStatement,

	// elif (x == y) { }
	ElifClause,

	// else { }
	ElseClause,

	// int x = 1, int x = 1, y = 2;
	VariableDeclaration,

	// the 'x = 1' in 'int x = 1'
	VariableDeclarator,

	// unsafe { }
	UnsafeStatement,

	// lock(obj) { }
	LockStatement,

	// -----------------------------
	// Clauses
	// -----------------------------

	// = value
	EqualsValueClause,

	// = int
	EqualsTypeClause,

	// => 1 + 1
	ArrowExpressionClause,

	// Value = 
	NameEqualsClause
}
