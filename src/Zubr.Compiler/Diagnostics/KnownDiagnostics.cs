using System.Diagnostics;

namespace Zubr.Compiler.Diagnostics;

internal static class KnownDiagnostics
{
	public static readonly string UnexpectedCharacter = "Unexpected character";

	public static readonly string UnexpectedToken = "Unexpected token";

	public static readonly string SyntaxError = "Syntax error";

	public static readonly string InvalidModifier = "Invalid modifier";

	public static readonly string ElseIfNotSupported = "Else if is not supported, use elif instead";

	public static readonly string UnexpectedEndOfFile = "Unexpected end of file";

	public static string GetMessage(ErrorCode code)
	{
		return code switch
		{
			ErrorCode.ERR_UnexpectedCharacter => UnexpectedCharacter,
			ErrorCode.ERR_UnexpectedToken => UnexpectedToken,
			ErrorCode.ERR_SyntaxError => SyntaxError,
			ErrorCode.ERR_InvalidModifier => InvalidModifier,
			ErrorCode.ERR_ElseIfNotSupported => ElseIfNotSupported,
			ErrorCode.ERR_UnexpectedEndOfFile => UnexpectedEndOfFile,
			_ => throw new UnreachableException()
		};
	}
}
