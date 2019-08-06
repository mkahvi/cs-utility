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
		readonly int Size;

		readonly T[] Ring;

		int ICollection<T>.Count { get => Size; }

		bool ICollection<T>.IsReadOnly { get => false; }

		public T[] Array() => Ring;

		public CircularBuffer(int size)
		{
			if (size < 2) throw new ArgumentException(nameof(size));

			Size = size;
			Ring = new T[Size];
		}

		public CircularBuffer(CircularBuffer<T> old, int size)
		{
			if (size < 2) throw new ArgumentException(nameof(size));

			Size = size;
			Ring = new T[Size];

			foreach (var value in old)
				Push(value);
		}

		public void Push(T value)
		{
			Ring[Index++ % Size] = value;
			if (Index < 0) Index = 0; // to avoid overflow
		}

		public T Tail() => Ring[Index];

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

		void ICollection<T>.Add(T item) => Push(item);

		void ICollection<T>.Clear() => throw new NotImplementedException("");

		bool ICollection<T>.Contains(T item)
		{
			foreach (var value in this)
			{
				if (value.Equals(item))
					return true;
			}

			return false;
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			uint offset = Index;
			int maxCopy = Math.Min(Size, array.Length - arrayIndex);

			for (int i = 0; i < maxCopy; i++)
				array[arrayIndex++] = this[offset++];
		}

		bool ICollection<T>.Remove(T item) => throw new NotImplementedException("remove not supported for circularbuffer");
	}
}
