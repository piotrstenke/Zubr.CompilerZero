namespace Zubr.Compiler.Diagnostics;

internal sealed class Diagnostic
{
	public ErrorCode Code { get;}

	public int Position { get; }

	public Diagnostic(ErrorCode code, int position)
	{
		Code = code;
		Position = position;
	}
}
