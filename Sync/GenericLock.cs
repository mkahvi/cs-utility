//
// MKAh.Synchronize.GenericLock.cs
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
	/// <summary>
	/// This lock does not care about thread context.
	/// </summary>
	public class GenericLock : ILock
	{
		private int _Lock = Atomic.Unlocked;

		public GenericLock()
		{
			// NOP
		}

		public int Queue { get; private set; } = 0;

		public bool Waiting => Queue > 0;

		public bool Blocking => true;

		public int Lock()
		{
			Queue++;
			TryLock();
			Queue--;

			return Queue;
		}

		public bool TryLock() => Atomic.Lock(ref _Lock);

		public bool IsLocked() => _Lock == Atomic.Locked;

		/// <summary>
		/// Wait for the lock to be released.
		/// </summary>
		public void Wait()
		{
			TryLock();
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
			return TryUnlock();
		}

		public bool TryUnlock()
		{
			Atomic.Unlock(ref _Lock);
			return true;
		}
	}
}
