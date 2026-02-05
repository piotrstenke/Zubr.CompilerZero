namespace zubr.interop.csharp;

public readonly struct CSharpArray<T>
{
	private readonly T[] _array;

	public int length => _array.Length;

	public T this[int index]
	{
		get => _array[index];
		set => _array[index] = value;
	}

	public CSharpArray(int length)
	{
		_array = new T[length];
	}

	public T[] copy()
	{
		T[] array = new T[_array.Length];
		_array.CopyTo(array, 0);
		return array;
	}
}
