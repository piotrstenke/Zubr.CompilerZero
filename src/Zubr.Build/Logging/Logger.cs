namespace Zubr.Build.Logging;

public static class Logger
{
	public static ILogger Null => NullLogger.Instance;

	public static ILogger Console(LogLevel minimalLevel)
	{
		return new ConsoleLogger(minimalLevel);
	}
}
