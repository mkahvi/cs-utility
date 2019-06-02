﻿//
// Human.Readable.Time.cs
//
// Author:
//       M.A. (https://github.com/mkahvi)
//
// Copyright (c) 2016–2019 M.A.
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

namespace MKAh.Human.Readable
{
	public static partial class Time
	{
		public enum Timescale
		{
			Seconds,
			Minutes,
			Hours,
		}

		public static string TimescaleString(Timescale t, bool plural = true)
			=> t switch
			{
				Timescale.Seconds => $"second{(plural ? "s" : "")}",
				Timescale.Minutes => $"minute{(plural ? "s" : "")}",
				Timescale.Hours => $"hour{(plural ? "s" : "")}",
				_ => string.Empty,
			};

		/// <summary>
		/// Returns very large time unit representing the time.
		/// </summary>
		/// <param name="seconds"></param>
		/// <param name="scale"></param>
		/// <returns></returns>
		public static double Simplify(TimeSpan time, out Timescale scale)
		{
			Debug.Assert(time.TotalSeconds >= 0);

			if (time.TotalHours > 2d)
			{
				scale = Timescale.Hours;
				return time.TotalHours;
			}
			else if (time.TotalMinutes > 2d)
			{
				scale = Timescale.Minutes;
				return time.TotalMinutes;
			}
			else
			{
				scale = Timescale.Seconds;
				return time.TotalSeconds;
			}
		}
	}
}
