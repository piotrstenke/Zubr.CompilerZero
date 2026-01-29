namespace Zubr.Build.Logging;

internal sealed class NullLogger : ILogger
{
	public static NullLogger Instance { get; } = new();

	LogLevel ILogger.MinimalLevel => LogLevel.None;

	private NullLogger()
	{
	}

	public void Log(LogLevel level, string message)
	{
		// Do nothing
	}
}
