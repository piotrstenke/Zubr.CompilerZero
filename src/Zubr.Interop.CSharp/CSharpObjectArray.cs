using System;
using System.ComponentModel;

namespace zubr.interop.csharp;

[Obsolete(Constants.ObsoleteMessage, DiagnosticId = Constants.ObsoleteDiagnosticId)]
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct CSharpObjectArray<T>
{
	private readonly Array _array;

	public int length => _array.Length;

	public int rank => _array.Rank;

	private CSharpObjectArray(Array array)
	{
		_array = array;
	}

	public T get(int index)
	{
		return (T)_array.GetValue(index)!;
	}

	public T get(int index1, int index2)
	{
		return (T)_array.GetValue(index1, index2)!;
	}

	public T get(int index1, int index2, int index3)
	{
		return (T)_array.GetValue(index1, index2, index3)!;
	}

	public T get(params int[] indices)
	{
		return (T)_array.GetValue(indices)!;
	}

	public void set(T value, int index)
	{
		_array.SetValue(value, index);
	}

	public void set(T value, int index1, int index2)
	{
		_array.SetValue(value, index1, index2);
	}

	public void set(T value, int index1, int index2, int index3)
	{
		_array.SetValue(value, index1, index2, index3);
	}

	public void set(T value, params int[] indices)
	{
		_array.SetValue(value, indices);
	}

	public int lengthAt(int dimension)
	{
		return _array.GetLength(dimension);
	}

	public static implicit operator CSharpObjectArray<T>(Array array)
	{
		return new(array);
	}

	public static implicit operator Array(CSharpObjectArray<T> array)
	{
		return array._array;
	}
}
