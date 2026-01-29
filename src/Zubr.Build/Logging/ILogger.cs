namespace Zubr.Build.Logging;

public interface ILogger
{
	LogLevel MinimalLevel { get; }

	void Log(LogLevel level, string message);
}
