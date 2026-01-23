namespace Zubr.Compiler.Diagnostics;

internal enum ErrorCode
{
	None = 0,

	ERR_UnexpectedCharacter,

	ERR_UnexpectedToken,

	ERR_SyntaxError,

	ERR_InvalidModifier,

	ERR_UnexpectedEndOfFile,
}
