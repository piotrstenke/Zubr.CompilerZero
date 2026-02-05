namespace Zubr.Build;

public static class ZubrRuntimeExtensions
{
	public static bool IsDotNet(this ZubrRuntime runtime)
	{
		return runtime.Name?.StartsWith("net") ?? false;
	}
}
