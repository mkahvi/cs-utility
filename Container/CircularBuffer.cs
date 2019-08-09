//
// CircularBuffer.cs
//
// Author:
//       M.A. (https://github.com/mkahvi)
//
// Copyright (c) 2019 M.A.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MKAh.Container
{
	public class CircularBuffer<T> : IEnumerable<T>, ICollection<T>
	{
		uint Index = 0;

		public int Size { get => Ring.Length; }

		public int Count { get => Size; }

		readonly T[] Ring;

		public int Offset => Convert.ToInt32(Index % Size);

		public bool IsReadOnly { get => false; }

		public T[] Array() => Ring;

		public CircularBuffer(int size)
		{
			if (size < 2) throw new ArgumentException(nameof(size));

			Ring = new T[Size];
		}

		public CircularBuffer(CircularBuffer<T> old, int size)
		{
			if (size < 2) throw new ArgumentException(nameof(size));

			Ring = new T[Size];

			foreach (var value in old)
				Add(value);
		}

		public T Current => Ring[Index];

		/// <summary>
		/// Please use unsigned int.
		/// </summary>
		public T this[int index]
		{
			get => this[Convert.ToUInt32(index)];
			set => this[Convert.ToUInt32(index)] = value;
		}

		public T this[uint index]
		{
			get => Ring[index % Size];
			set => Ring[index % Size] = value;
		}

		public IEnumerator<T> GetEnumerator()
		{
			uint lindex = Index;

			for (int i = 0; i < Size; i++)
				yield return Ring[lindex++ % Size];
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(T value) => Ring[Index++ % Size] = value;

		public void Clear() => throw new NotImplementedException("");

		public T Get(int offset) => Ring[Index + offset];

		public T[] GetRange(int offset, int length)
		{
			uint lindex = Index + Convert.ToUInt32(Ring.Length + offset);

			if (length < 1 || length > Size) throw new ArgumentException(nameof(length));

			var rv = new T[length];

			for (int i = 0; i < length; i++)
				rv[i] = Ring[lindex++ % Size];

			return rv;
		}

		public bool Contains(T item)
		{
			foreach (var value in this)
			{
				if (value.Equals(item))
					return true;
			}

			return false;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			uint offset = Index;
			int maxCopy = Math.Min(Size, array.Length - arrayIndex);

			for (int i = 0; i < maxCopy; i++)
				array[arrayIndex++] = this[offset++];
		}

		public bool Remove(T item) => throw new NotImplementedException("remove not supported for circularbuffer");
	}
}
