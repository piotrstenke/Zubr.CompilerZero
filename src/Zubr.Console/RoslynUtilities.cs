using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Numerics;

namespace Zubr;

public static class RoslynUtilities
{
	public static CSharpCompilation CreateCompilation(params SyntaxTree[]? syntaxTrees)
	{
		MetadataReference[] references = GetBaseReferences();

		return CSharpCompilation.Create(
			assemblyName: "Zubr.dll",
			syntaxTrees: syntaxTrees,
			references: references,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
		);
	}

	static MetadataReference[] GetBaseReferences()
	{
		string directory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

		string[] locations = new string[]
		{
			typeof(Console).Assembly.Location,
			typeof(object).Assembly.Location,
			typeof(File).Assembly.Location,
			typeof(BigInteger).Assembly.Location,
			typeof(Enumerable).Assembly.Location,
			typeof(List<>).Assembly.Location,
			Path.Combine(directory, "System.Runtime.dll")
		};

		List<MetadataReference> references = new(locations.Length);

		foreach (string location in locations)
		{
			references.Add(MetadataReference.CreateFromFile(location));
		}

		return references.ToArray();
	}
}
