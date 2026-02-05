using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Zubr.Build;

[DebuggerDisplay("{Name,nq}")]
public readonly struct ZubrRuntime : IEquatable<ZubrRuntime>
{
	public const string Dotnet10 = "net10";

	public string Name { get; }

	internal ZubrRuntime(string name)
	{
		Name = name;
	}

	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return obj is ZubrRuntime other && Equals(other);
	}

	public bool Equals(ZubrRuntime other)
	{
		return Name == other.Name;
	}

	public override string ToString()
	{
		return Name;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	public static bool operator ==(ZubrRuntime left, ZubrRuntime right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ZubrRuntime left, ZubrRuntime right)
	{
		return !left.Equals(right);
	}

	public static bool operator ==(ZubrRuntime left, string right)
	{
		return left.Name == right;
	}

	public static bool operator !=(ZubrRuntime left, string right)
	{
		return left.Name != right;
	}

	public static bool operator ==(string left, ZubrRuntime right)
	{
		return left == right.Name;
	}

	public static bool operator !=(string left, ZubrRuntime right)
	{
		return left != right.Name;
	}

	public static ZubrRuntime Parse(string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(value);

		if(!TryParseImpl(value, out ZubrRuntime runtime))
		{
			throw new ArgumentException($"Value '{value}' is not a valid {nameof(ZubrRuntime)} value");
		}

		return runtime;
	}

	public static bool TryParse([NotNullWhen(true)] string? value, out ZubrRuntime result)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			result = default;
			return false;
		}

		return TryParseImpl(value, out result);
	}

	private static bool TryParseImpl(string value, out ZubrRuntime result)
	{
		if (value.Equals(Dotnet10, StringComparison.OrdinalIgnoreCase))
		{
			result = new(Dotnet10);
			return true;
		}

		result = default;
		return false;
	}
}
