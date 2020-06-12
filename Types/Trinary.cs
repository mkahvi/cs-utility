//
// MKAh.Types.Trinary.cs
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

namespace MKAh.Types
{
	public struct Trinary : IEquatable<Trinary>
	{
		internal State Value;

		public static Trinary FromBool(bool value) => new Trinary { Value = value ? State.True : State.False };

		public bool IsTrue => Value == State.True;
		public bool IsFalse => Value == State.False;
		public bool IsNonce => Value == State.Nonce;
		public static Trinary True => new Trinary { Value = State.True };
		public static Trinary False => new Trinary { Value = State.False };
		public static Trinary Nonce => new Trinary { Value = State.Nonce };
		public Trinary SetTrue() => SetValue(State.True);
		public Trinary SetFalse() => SetValue(State.False);
		public Trinary SetNonce() => SetValue(State.Nonce);

		public Trinary SetValue(State value)
		{
			Value = value;
			return this;
		}

		public Trinary(bool value) => Value = value ? State.True : State.False;
		public Trinary(Trinary trinary) => Value = trinary.Value;

		public bool Bool => Value == State.True;

		public static implicit operator Trinary(bool value)
			=> new Trinary { Value = value ? State.True : State.False };

		public static Trinary FromBoolean(bool value) => new Trinary { Value = value ? State.True : State.False };

		public static implicit operator Trinary(int value)
			=> new Trinary { Value = (State)value };

		public static Trinary FromInt32(int value) => new Trinary { Value = (State)value };

		public static bool operator ==(Trinary left, Trinary right) => left.Equals(right);

		public static bool operator !=(Trinary left, Trinary right) => !(left == right);

		public static bool operator ==(Trinary left, bool right) => (right && left.IsTrue) || (!right && left.IsFalse);
		public static bool operator !=(Trinary left, bool right) => !(left == right);
		public static bool operator ==(bool left, Trinary right) => right == left;
		public static bool operator !=(bool left, Trinary right) => right != left;

		public static implicit operator bool(Trinary trinary) => trinary.IsTrue;

		public bool ToBoolean() => IsTrue;

		public static implicit operator int(Trinary trinary) => (int)trinary.Value;

		public int ToInt32() => (int)Value;

		public bool Equals(Trinary other) => Value == other.Value;

		public bool Equals(bool other) => Bool == other;

		// Mixed 
		public override int GetHashCode() => -1937169414 + Value.GetHashCode();

		public override bool Equals(object obj)
		{
			if (obj is Trinary trinary)
				return Equals(trinary);
			else if (obj is bool boolean)
				return Equals(boolean);
			return base.Equals(obj);
		}

		public override string ToString() => Value switch
		{
			State.True => bool.TrueString,
			State.False => bool.FalseString,
			_ => NonceString,
		};

		public enum State
		{
			False = 0,
			True = 1,
			Nonce = -1
		}

		public static readonly string NonceString = "Nonce";
	}
}