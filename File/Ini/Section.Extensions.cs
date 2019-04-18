//
// Ini.Section.Extensions.cs
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

using System.Diagnostics;

namespace MKAh.Ini
{
	public static partial class HelperExtensions
	{
		public static Ini.Setting GetOrSet<T>(this Ini.Section section, string setting, T[] fallback, out bool defaulted)
		{
			Ini.Setting rv = null;

			if (section.TryGet(setting, out rv) && rv.Array != null)
				defaulted = false;
			else
			{
				if (rv == null) section.Add(rv = new Ini.Setting() { Name = setting });

				rv.SetArray(Ini.Converter<T>.Convert(fallback));
				defaulted = true;
			}

			return rv;
		}

		public static Ini.Setting GetOrSet<T>(this Ini.Section section, string setting, T fallback, out bool defaulted)
		{
			Debug.Assert(section != null);
			Debug.Assert(!string.IsNullOrEmpty(setting));

			Ini.Setting rv = null;

			if (section.TryGet(setting, out rv) && rv.Value != null)
				defaulted = false;
			else
			{
				if (rv == null) section.Add(rv = new Ini.Setting() { Name = setting });

				rv.Set(Ini.Converter<T>.Convert(fallback));

				// TODO: signal owning config that this has been changed

				defaulted = true;
			}

			return rv;
		}
	}
}
