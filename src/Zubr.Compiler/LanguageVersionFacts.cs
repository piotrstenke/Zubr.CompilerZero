using System;

namespace Zubr.Compiler;

public static class LanguageVersionFacts
{
	public static string ToDisplayString(this LanguageVersion version)
	{
		return version switch
		{
			LanguageVersion.Default => "default",
			LanguageVersion.Alpha => "alpha",
			LanguageVersion.Latest => "latest",
			LanguageVersion.Preview => "preview",
			_ => throw new ArgumentException($"Unsupported value '{version}' of type '{nameof(LanguageVersion)}", nameof(version))
		};
	}

	public static LanguageVersion GetEffectiveVersion(this LanguageVersion version)
	{
		return version switch
		{
			LanguageVersion.Default or
			LanguageVersion.Latest or
			LanguageVersion.Preview
				=> LanguageVersion.Alpha,

			_ => version
		};
	}
}
