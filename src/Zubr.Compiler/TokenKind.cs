namespace Zubr.Compiler;

public enum TokenKind : uint
{
	// Not a token.
	None,

	// =
	EqualsToken,

	// ==
	EqualsEqualsToken,

	// ===
	EqualsEqualsEqualsToken,

	// =>
	EqualsGreaterThanToken,

	// !
	ExclamationToken,

	// !=
	ExclamationEqualsToken,

	// !==
	ExclamationEqualsEqualsToken,

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

	// class
	ClassKeyword,

	// struct
	StructKeyword,

	// trait
	TraitKeyword,

	// impl
	ImplKeyword,

	// union
	UnionKeyword,

	// alias
	AliasKeyword,

	// enum
	EnumKeyword,

	// attr
	AttrKeyword,

	// field
	FieldKeyword,

	// oper
	OperKeyword,

	// cast
	CastKeyword,

	// auto
	AutoKeyword,

	// data
	DataKeyword,

	// open
	OpenKeyword,

	// // closed
	// ClosedKeyword,

	// limit
	LimitKeyword,

	// base
	BaseKeyword,

	// self
	SelfKeyword,

	// pub
	PubKeyword,

	// prot
	ProtKeyword,

	// scoped
	ScopedKeyword,

	// priv
	PrivKeyword,

	// stat
	StatKeyword,

	// over
	OverKeyword,

	// final
	FinalKeyword,

	// // extern
	// ExternKeyword,

	// mut
	MutKeyword,

	// req
	ReqKeyword,

	// const
	ConstKeyword,

	// hold
	HoldKeyword,

	// flag
	FlagKeyword,

	// new
	NewKeyword,

	// free
	FreeKeyword,

	// gcfree
	GCFreeKeyword,

	// get
	GetKeyword,

	// set
	SetKeyword,

	// init
	InitKeyword,

	// value
	ValueKeyword,

	// let
	LetKeyword,

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

	// stop
	StopKeyword,

	// next
	NextKeyword,

	// return
	ReturnKeyword,

	// is
	IsKeyword,

	// and
	AndKeyword,

	// not
	NotKeyword,

	// or
	OrKeyword,

	// where
	WhereKeyword,

	// null
	NullKeyword,

	// file
	FileKeyword,

	// assembly
	AssemblyKeyword,

	// true
	TrueKeyword,

	// false
	FalseKeyword,

	// bool
	BoolKeyword,

	// any
	AnyKeyword,

	// void
	VoidKeyword,

	// sbyte
	SByteKeyword,

	// short
	ShortKeyword,

	// int
	IntKeyword,

	// long
	LongKeyword,

	// byte
	ByteKeyword,

	// ushort
	UShortKeyword,

	// uint
	UIntKeyword,

	// ulong
	ULongKeyword,

	// nint
	NIntKeyword,

	// nuint
	NUIntKeyword,

	// half
	HalfKeyword,

	// float
	FloatKeyword,

	// double
	DoubleKeyword,

	// decimal
	DecimalKeyword,

	// char
	CharKeyword,

	// string
	StringKeyword,

	// An empty token that is valid but has no representation in the source.
	SkippedToken,

	// A token that was expected but is missing.
	MissingToken,

	// A token that is unexpected or invalid.
	BadToken,

	// End of file
	EOF = uint.MaxValue
}
