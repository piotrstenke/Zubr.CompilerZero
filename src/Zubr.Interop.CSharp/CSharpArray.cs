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

	private CSharpArray(T[] array)
	{
		_array = array;
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

	public T[] copy(int length)
	{
		T[] array = new T[length];

		for (int i = 0; i < length; i++)
		{
			array[i] = _array[i];
		}

		return array;
	}

	public static implicit operator CSharpArray<T>(T[] array)
	{
		return new(array);
	}
}
