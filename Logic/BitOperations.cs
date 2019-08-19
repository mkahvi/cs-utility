//
// MKAh.Logic.BitOperations.cs
//
// Author:
//       M.A. (https://github.com/mkahvi)
//
// Copyright (c) 2018 M.A.
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

using System.Runtime.CompilerServices;

namespace MKAh.Logic
{
	public static class Bit
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Set(int dec, int index) => Or(dec, (1 << index));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsSet(int value, int index) => And(value, (1 << index)) != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Unset(int value, int index) => And(value, ~(1 << index));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Or(int value1, int value2) => value1 | value2;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int And(int value1, int value2) => value1 & value2;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Count(int i)
		{
			i -= ((i >> 1) & 0x55555555);
			i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
			return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
		}

		// This looks really slow
		/// <summary>
		/// Fills in unset bits up to maximum number of of bits.
		/// </summary>
		/// <param name="value">Value to adjust.</param>
		/// <param name="allowedbits">Mask of allowed bit positions.</param>
		/// <param name="maxbits">Maximum number of enabled bits.</param>
		// TODO: More approaches to bit filling than just linear.
		public static int Fill(int value, int allowedbits, int maxbits)
		{
			//var bits = Count(value);

			int filled = 0;

			for (int i = 0; i < 32 && filled <= maxbits; i++)
			{
				if (IsSet(allowedbits, i) && !IsSet(value, i))
				{
					value = Set(value, i);
					//bits++;
					filled++;
				}
			}

			return value;
		}

		public static int Unfill(int value, int allowedBits, int maxBits)
		{
			int unfilled = 0;

			for (int i = 0; i < 32 && unfilled <= maxBits; i++)
			{
				if (!IsSet(allowedBits, i) && IsSet(value, i))
				{
					value = Unset(value, i);
					//bits++;
					unfilled++;
				}
			}

			return value;
		}
	}
}
