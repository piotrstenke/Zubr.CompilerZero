using System;

namespace Zubr.Build.Logging;

internal sealed class ConsoleLogger : ILogger
{
	public LogLevel MinimalLevel { get; }

	public ConsoleLogger(LogLevel minimalLevel)
	{
		MinimalLevel = minimalLevel;
	}

	public void Log(LogLevel level, string message)
	{
		if(level >= MinimalLevel)
		{
			Console.WriteLine($"[{level}]: {message}");
		}
	}
}
