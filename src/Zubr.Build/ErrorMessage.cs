namespace Zubr.Build;

public sealed class ErrorMessage
{
	public required string Message { get; set; }

	public required ErrorLevel Level { get; set; }

	public required int Line { get; set; }

	public required int Column { get; set; }
}
