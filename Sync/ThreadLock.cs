//
// MKAh.Synchronize.Scoping.cs
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

namespace MKAh.Synchronize
{
	public class ThreadLock : ILock
	{
		readonly object _Lock = new object();

		public ThreadLock()
		{
			// NOP
		}

		public int Queue { get; private set; } = 0;

		public bool Waiting => Queue > 0;

		public bool Blocking => true;

		public int Lock()
		{
			Queue++;
			System.Threading.Monitor.Enter(_Lock);
			Queue--;
			if (disposed) throw new ObjectDisposedException(nameof(ThreadLock), "Lock entered after dispose");
			return Queue;
		}

		public bool TryLock() => System.Threading.Monitor.TryEnter(_Lock);

		public bool IsLocked() => System.Threading.Monitor.IsEntered(_Lock);

		/// <summary>
		/// Wait for the lock to be released.
		/// </summary>
		public void Wait()
		{
			Lock();
			Unlock();
		}

		public ScopedUnlock ScopedLock()
		{
			Lock();

			return new ScopedUnlock(this);
		}

		public ScopedUnlock ScopedUnlock() => new ScopedUnlock(this);

		public ScopedUnlock? TryScopedLock()
		{
			if (TryLock())
				return new ScopedUnlock(this);

			return null;
		}

		public bool Unlock()
		{
			if (disposed) throw new ObjectDisposedException(nameof(ThreadLock), "Lock exited after dispose");

			System.Threading.Monitor.Pulse(_Lock);
			System.Threading.Monitor.Exit(_Lock);

			return true;
		}

		public bool TryUnlock() => Unlock();

		#region IDisposable Support
		private bool disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;
			disposed = true;

			if (disposing)
			{
				//base.Dispose();
			}

			Unlock();
		}

		public void Dispose() => Dispose(true);
		#endregion
	}
}
