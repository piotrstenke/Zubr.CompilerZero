namespace Zubr.Compiler.Diagnostics;

internal sealed class InternalDiagnostic
{
	public ErrorCode Code { get; }

	public int Position { get; }

	public InternalDiagnostic(ErrorCode code, int position)
	{
		Code = code;
		Position = position;
	}
}
