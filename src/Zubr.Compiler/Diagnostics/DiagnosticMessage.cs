namespace Zubr.Compiler.Diagnostics;

public sealed class DiagnosticMessage
{
	public string Code { get; }

	public int Position { get; }

	public string Message { get; }

	public DiagnosticSeverity Severity { get; }

	public string? SourceFile { get; }

	internal DiagnosticMessage(
		string code,
		int position,
		string message,
		DiagnosticSeverity severity,
		string? sourceFile
	)
	{
		Code = code;
		Position = position;
		Message = message;
		Severity = severity;
		SourceFile = sourceFile;
	}
}
