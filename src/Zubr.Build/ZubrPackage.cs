using Tomlyn;

namespace Zubr.Build;

public sealed class ZubrPackage
{
	[TomlPropertyName("name")]
	public string? Name { get; set; }

	[TomlPropertyName("version")]
	public string? Version { get; set; }

	[TomlPropertyName("license")]
	public string? License { get; set; }

	[TomlPropertyName("url")]
	public string? Url { get; set; }

	[TomlPropertyName("authors")]
	public string[]? Authors { get; set; }
}
