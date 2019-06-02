﻿//
// MKAh.Execution.User.cs
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
using System.Threading;

namespace MKAh
{
	public static partial class Execution
	{
		static bool? isAdmin = null;
		public static bool IsAdministrator
		{
			get
			{
				if (!isAdmin.HasValue)
				{
					// https://stackoverflow.com/a/10905713
					using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
					{
						var principal = new System.Security.Principal.WindowsPrincipal(identity);
						isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
					}
				}

				return isAdmin.Value;
			}
		}

		/// <summary>
		/// Returns true if OS version is 6.1 and platform is NT.
		/// </summary>
		public static bool IsWin7 => (System.Environment.OSVersion.Platform == PlatformID.Win32NT && System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 1);

		/// <summary>
		/// Returns true if OS version is higher than 6.2
		/// </summary>
		public static bool IsWin8OrBetter
			=> (System.Environment.OSVersion.Platform == PlatformID.Win32NT
			&& (System.Environment.OSVersion.Version.Major > 6)
			|| (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Minor == 2));

		/// <summary>
		/// Current code is executing on the main thread.
		/// </summary>
		public static bool IsMainThread => Thread.CurrentThread.IsThreadPoolThread == false && Thread.CurrentThread.ManagedThreadId == 1;

		// For figuring out multi-threading related problems
		// From StarOverflow: https://stackoverflow.com/q/22579206
		[Conditional("DEBUG")]
		public static void ThreadIdentity(string message = "")
		{
			var thread = Thread.CurrentThread;
			string name = thread.IsThreadPoolThread ? "Thread pool" : (string.IsNullOrEmpty(thread.Name) ? $"#{thread.ManagedThreadId.ToString()}" : thread.Name);
			Debug.WriteLine($"Continuation on: {name} --- {message}");
		}
	}
}
