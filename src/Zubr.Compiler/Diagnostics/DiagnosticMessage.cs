using Zubr.Compiler.Text;

namespace Zubr.Compiler.Diagnostics;

public sealed class DiagnosticMessage
{
	public string Code { get; }

	public string Message { get; }

	public DiagnosticSeverity Severity { get; }

	public Location Location { get; }

	internal DiagnosticMessage(
		string code,
		string message,
		DiagnosticSeverity severity,
		Location location
	)
	{
		Code = code;
		Message = message;
		Severity = severity;
		Location = location;
	}
}
