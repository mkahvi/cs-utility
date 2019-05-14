//
// Ini.Setting.cs
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
using System.Diagnostics;

namespace MKAh.Ini
{
	public class Setting : Interface.Value
	{
		public SettingType Type { get; private set; } = SettingType.Generic;

		public int Index { get; internal set; } = 0;

		public Setting(SettingType type = SettingType.Generic, Section parent = null)
		{
			ResetEscapedCache();
			Type = type;
			Parent = parent;
		}

		public Section Parent { get; set; } = null;

		Type LastType = typeof(string);

		protected override void Altered() => Parent?.ChildAltered(this);

		public override string ToString()
		{
			if (CommentOnly)
				return $"# {Comment}";
			else if (string.IsNullOrEmpty(Comment))
				return $"{Name} = {EscapedValue}";
			else
				return $"{Name} = {EscapedValue} # {Comment}";
		}

		string _value = null;
		public string Value
		{
			get => _value;
			set
			{
				ResetEscapedCache();
				_array = null;
				_value = value;
				Altered();
			}
		}

		// alias for Value
		public string String
		{
			get => Value;
			set => Value = value;
		}

		public bool Bool
		{
			set => Set(value);
			get => bool.Parse(Value);
		}

		public int Int
		{
			set => Set(value);
			get => int.Parse(Value, System.Globalization.NumberStyles.Integer | System.Globalization.NumberStyles.AllowThousands);
		}

		public float Float
		{
			set => Set(value);
			get => float.Parse(Value, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands);
		}

		public double Double
		{
			set => Set(value);
			get => double.Parse(Value, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands);
		}

		public int[] IntArray
		{
			set => SetArray(value);
			get
			{
				if ((Array?.Length ?? 0) == 0) return null;

				int[] array = new int[Array.Length];

				for (int i = 0; i < Array.Length; i++)
					array[i] = int.Parse(Array[i].Trim(), System.Globalization.NumberStyles.Integer | System.Globalization.NumberStyles.AllowThousands);

				return array;
			}
		}

		public float[] FloatArray
		{
			set => SetArray(value);
			get
			{
				if ((Array?.Length ?? 0) == 0) return null;

				float[] array = new float[Array.Length];

				for (int i = 0; i < Array.Length; i++)
					array[i] = float.Parse(Array[i].Trim(), System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands);

				return array;
			}
		}

		public double[] DoubleArray
		{
			set => SetArray(value);
			get
			{
				if ((Array?.Length ?? 0) == 0) return null;

				double[] array = new double[Array.Length];

				for (int i = 0; i < Array.Length; i++)
					array[i] = double.Parse(Array[i].Trim(), System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands);

				return array;
			}
		}

		Lazy<string> escapedValueCache = null;

		void ResetEscapedCache() => escapedValueCache = new Lazy<string>(CreateEscapedCache);

		protected string EscapedValue => escapedValueCache.Value;

		string _comment = null;
		public string Comment
		{
			get => _comment;
			set
			{
				_comment = value?.Replace('\n', ' '); // HACK: Multiline comments are not handled
				Altered();
			}
		}

		public bool CommentOnly => string.IsNullOrEmpty(Name);

		public bool IsArray => (Array?.Length ?? 0) > 0;

		string[] _array = null;
		public string[] Array
		{
			get => _array;
			set
			{
				_array = null;
				_value = null;

				ResetEscapedCache();

				_array = (string[])value?.Clone(); // is this enough?

				//Debug.WriteLine("BaseArray = " + string.Join(", ", value));
				Altered();
			}
		}

		// alias for Array
		public string[] StringArray
		{
			get => Array;
			set => Array = value;
		}

		string CreateEscapedCache()
		{
			if ((Array?.Length ?? 0) > 0)
			{
				string[] cache = new string[Array.Length];

				for (int i = 0; i < Array.Length; i++)
					cache[i] = EscapeValue(Array[i], out string nv) ? nv : Array[i];

				return $"{Constant.ArrayStart} " + string.Join(", ", cache) + $" {Constant.ArrayEnd}";
			}
			else if (!string.IsNullOrEmpty(Value))
				return EscapeValue(Value, out string nv) ? nv : Value;
			else
				return string.Empty;
		}

		public static bool EscapeValue(string value, out string nvalue)
		{
			Debug.Assert(!string.IsNullOrEmpty(value), nameof(value));

			bool NeedsEscaping = value.IndexOf(Data.Constant.Quote) >= 0;
			bool NeedsQuotes = value.IndexOfAny(Config.ReservedCharacters) >= 0;
			bool ExcessWhitespace = char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[value.Length - 1]);
			NeedsQuotes |= ExcessWhitespace;

			if (NeedsQuotes || NeedsEscaping)
			{
				nvalue = value;

				if (NeedsEscaping)
					nvalue = nvalue.Replace("\"", "\\\"");
				if (NeedsQuotes)
					nvalue = Data.Constant.Quote + nvalue + Data.Constant.Quote;

				return true;
			}

			nvalue = null;
			return false;
		}

		public static bool UnescapeValue(string value, out string nvalue, bool trim = false)
		{
			if (string.IsNullOrEmpty(value) || value.Length == 0)
			{
				nvalue = null;
				return false;
			}

			if (trim) value = value.Trim();

			bool NeedsUnEscaping = value.IndexOf(Data.Constant.EscapeChar) >= 0;
			bool NeedsUnQuoting = value[0].Equals(Data.Constant.Quote);

			if (NeedsUnEscaping || NeedsUnQuoting || trim)
			{
				nvalue = value;

				if (NeedsUnQuoting)
					nvalue = nvalue.Substring(1, nvalue.Length - 2);
				if (NeedsUnEscaping)
					nvalue = nvalue.Replace("\\\"", "\"");

				if (trim) nvalue = nvalue.Trim();

				return true;
			}

			nvalue = null;
			return false;
		}

		internal string[] UnescapeArray(string[] values)
		{
			string[] nv = new string[values.Length];

			//Debug.WriteLine("UnescapingArray: \"" + string.Join("\", \"", values) + "\"");

			for (int i = 0; i < values.Length; i++)
				nv[i] = UnescapeValue(values[i], out string nsv, trim: true) ? nsv : values[i];

			//Debug.WriteLine("UnescapedArray:  \"" + string.Join("\", \"", nv) + "\"");

			return nv;
		}

		public void SetArray(string[] array) => Array = array;
		public void Set(string value) => Value = value;
		public void SetArray(bool[] values) => Array = Converter<bool>.Convert(values);
		public void Set(bool value) => Value = Converter<bool>.Convert(value);
		public void SetArray(int[] values) => Array = Converter<int>.Convert(values);
		public void Set(int value) => Value = Converter<int>.Convert(value);
		public void SetArray(uint[] values) => Array = Converter<uint>.Convert(values);
		public void Set(uint value) => Value = Converter<uint>.Convert(value);
		public void SetArray(long[] values) => Array = Converter<long>.Convert(values);
		public void Set(long value) => Value = Converter<long>.Convert(value);
		public void SetArray(ulong[] values) => Array = Converter<ulong>.Convert(values);
		public void Set(ulong value) => Value = Converter<ulong>.Convert(value);
		public void SetArray(float[] values) => Array = Converter<float>.Convert(values);
		public void Set(float value) => Value = Converter<float>.Convert(value);
		public void SetArray(double[] values) => Array = Converter<double>.Convert(values);
		public void Set(double value) => Value = Converter<double>.Convert(value);
		public void SetArray(decimal[] values) => Array = Converter<decimal>.Convert(values);
		public void Set(decimal value) => Value = Converter<decimal>.Convert(value);

		/// <summary>
		/// Sets comment only if it's empty.
		/// </summary>
		public Setting InitComment(string comment)
		{
			if (string.IsNullOrEmpty(Comment))
				Comment = comment;

			return this;
		}

		/// <summary>
		/// Shitty LINQ style comment setting.
		/// </summary>
		public Setting PutComment(string comment)
		{
			Comment = comment;

			return this;
		}
	}
}
