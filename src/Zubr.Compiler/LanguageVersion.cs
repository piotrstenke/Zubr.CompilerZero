namespace Zubr.Compiler;

public enum LanguageVersion
{
	Default = 0,

	Alpha = 1,

	LatestMajor = int.MaxValue - 2,

	Latest = int.MaxValue - 1,

	Preview = int.MaxValue,
}
