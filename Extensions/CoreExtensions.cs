//
// CoreExtensions.cs
//
// Author:
//       M.A. (https://github.com/mkahvi)
//
// Copyright (c) 2018–2019 M.A.
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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace MKAh
{
	static public class CoreTypeExtensions
	{
		/// <summary>
		/// Returns true if as equal as possible.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static bool RoughlyEqual(this double value, double other) => Math.Abs(value - other) <= double.Epsilon;

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static bool RoughlyEqual(this float value, float other) => Math.Abs(value - other) <= float.Epsilon;

		// Core Type extension
		/// <summary>
		/// Limits the number within stated inclusive minimum and maximum values.
		/// 
		/// <code>int.Constrain(minimum, maximum)</code>
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static int Constrain(this int value, int InclusiveMinimum, int InclusiveMaximum)
			=> Math.Min(Math.Max(value, InclusiveMinimum), InclusiveMaximum);

		/// <summary>
		/// Limits the number within stated inclusive minimum and maximum values.
		/// 
		/// <code>long.Constrain(minimum, maximum)</code>
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static long Constrain(this long value, long InclusiveMinimum, long InclusiveMaximum)
			=> Math.Min(Math.Max(value, InclusiveMinimum), InclusiveMaximum);

		/// <summary>
		/// Limits the number within stated inclusive minimum and maximum values.
		/// 
		/// <code>float.Constrain(minimum, maximum)</code>
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static float Constrain(this float value, float InclusiveMinimum, float InclusiveMaximum)
			=> Math.Min(Math.Max(value, InclusiveMinimum), InclusiveMaximum);

		/// <summary>
		/// Limits the number within stated inclusive minimum and maximum values.
		/// 
		/// <code>double.Constrain(minimum, maximum)</code>
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static double Constrain(this double value, double InclusiveMinimum, double InclusiveMaximum)
			=> Math.Min(Math.Max(value, InclusiveMinimum), InclusiveMaximum);

		/// <summary>
		/// Limits the number within stated inclusive minimum and maximum values.
		/// 
		/// <code>decimal.Constrain(minimum, maximum)</code>
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static decimal Constrain(this decimal value, decimal InclusiveMinimum, decimal InclusiveMaximum)
			=> Math.Min(Math.Max(value, InclusiveMinimum), InclusiveMaximum);

		/// <summary>
		/// Constrains inclusive maximum value.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static int Max(this int value, int max) => Math.Min(value, max);

		/// <summary>
		/// Constrains inclusive maximum value.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static long Max(this long value, long max) => Math.Min(value, max);

		/// <summary>
		/// Constrains inclusive maximum value.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static float Max(this float value, float max) => Math.Min(value, max);

		/// <summary>
		/// Constrains inclusive maximum value.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static double Max(this double value, double max) => Math.Min(value, max);

		/// <summary>
		/// Constrains inclusive maximum value.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static TimeSpan Max(this TimeSpan value, TimeSpan max) => value > max ? max : value;

		/// <summary>
		/// Returns true if value within the inclusive minimum and maximum values.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static bool Within(this int value, int low, int high) => value >= low && value <= high;

		/// <summary>
		/// Returns true if value within the inclusive minimum and maximum values.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static bool Within(this long value, long low, long high) => value >= low && value <= high;

		/// <summary>
		/// Returns true if value within the inclusive minimum and maximum values.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static bool Within(this float value, float low, float high) => value >= low && value <= high;

		/// <summary>
		/// Returns true if value within the inclusive minimum and maximum values.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static bool Within(this double value, double low, double high) => value >= low && value <= high;

		/// <summary>
		/// Constrains value to have inclusive minimum.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static int Min(this int value, int min) => Math.Max(value, min);

		/// <summary>
		/// Constrains value to have inclusive minimum.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static long Min(this long value, long min) => Math.Max(value, min);

		/// <summary>
		/// Constrains value to have inclusive minimum.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static float Min(this float value, float min) => Math.Max(value, min);

		/// <summary>
		/// Constrains value to have inclusive minimum.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static double Min(this double value, double min) => Math.Max(value, min);

		/// <summary>
		/// Replace value with another if it matches, otherwise return as is.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static int Replace(this int value, int from, int to) => value == from ? to : value;

		/// <summary>
		/// Replace value with another if it matches, otherwise return as is.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static long Replace(this long value, long from, long to) => value == from ? to : value;

		// IP Address
		public static IPAddress GetAddress(this NetworkInterface iface)
		{
			Debug.Assert(iface != null);

			return GetAddresses(iface)[0] ?? IPAddress.None;
		}

		public static IPAddress[] GetAddresses(this NetworkInterface iface)
		{
			Debug.Assert(iface != null);

			if (iface.NetworkInterfaceType == NetworkInterfaceType.Loopback || iface.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
				return Array.Empty<IPAddress>();

			return (from ip in iface.GetIPProperties().UnicastAddresses
					select ip.Address).ToArray();
		}
	}

	static public class PriorityClassExtensions
	{
		public static int ToInt32(this ProcessPriorityClass pc)
			=> pc switch
			{
				ProcessPriorityClass.Idle => 0,
				ProcessPriorityClass.BelowNormal => 1,
				//ProcessPriorityClass.Normal => 2,
				ProcessPriorityClass.AboveNormal => 3,
				ProcessPriorityClass.High => 4,
				//ProcessPriorityClass.RealTime => 5,
				_ => 2,
			};
	}

	static public class DateExtensions
	{
		//static readonly DateTime UnixEpoch = new new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

		//public static long Unixstamp(this DateTime dt) => Convert.ToInt64(dt.ToUniversalTime().Subtract(UnixEpoch).TotalSeconds);
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static long Unixstamp(this DateTimeOffset dt) => Convert.ToInt64(dt.Subtract(UnixEpoch).TotalSeconds);

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static DateTimeOffset Unixstamp(this long unixstamp) => UnixEpoch + TimeSpan.FromSeconds(unixstamp);

		/// <summary>
		/// Time since, because (now - since) just confuses me.
		/// <para>BUG: Same timezone required.</para>
		/// </summary>
		//public static TimeSpan TimeSince(this DateTime now, DateTime since) => (now - since);
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static TimeSpan Since(this DateTimeOffset now, DateTimeOffset since) => now - since;

		/// <summary>
		/// <para>BUG: Same timezone required.</para>
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static TimeSpan Since(this DateTime now, DateTime since) => now - since;

		/// <summary>
		/// <para>BUG: Same timezone required.</para>
		/// </summary>
		//public static TimeSpan TimeTo(this DateTime now, DateTime to) => (to - now);
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static TimeSpan To(this DateTimeOffset now, DateTimeOffset to) => to - now;

		/// <summary>
		/// <para>BUG: Same timezone required.</para>
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static TimeSpan To(this DateTime now, DateTime to) => to - now;
	}
}