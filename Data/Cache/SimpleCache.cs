//
// Data.Cache.SimpleCache.cs
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
using System.Collections.Concurrent;
using System.Linq;

namespace MKAh.Cache
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="KeyT">Access key.</typeparam>
	/// <typeparam name="ValueT">Value.</typeparam>
	public class SimpleCache<KeyT, ValueT> : IDisposable where ValueT : class
	{
		readonly EvictStrategy CacheEvictStrategy;
		readonly StoreStrategy CacheStoreStrategy;

		readonly ConcurrentDictionary<KeyT, CacheItem<KeyT, ValueT>> Items;

		public int Count => Items.Count;
		public ulong Hits { get; private set; } = 0;
		public ulong Misses { get; private set; } = 0;

		readonly System.Timers.Timer PruneTimer;

		int Overflow, AbsoluteCapacity, MinCache;
		int MaxAge = 60, MinAge = 5;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="limit">Attempt to have no more than this many items.</param>
		/// <param name="retention">Do not prune if this many or less items.</param>
		/// <param name="interval">Automatic pruning interval.</param>
		/// <param name="store"></param>
		/// <param name="evict"></param>
		public SimpleCache(int limit = 100, int retention = 10, TimeSpan? interval = null,
					 StoreStrategy store = StoreStrategy.ReplaceNoMatch, EvictStrategy evict = EvictStrategy.LeastRecent)
		{
			CacheStoreStrategy = store;
			CacheEvictStrategy = evict;

			AbsoluteCapacity = limit;
			MinCache = retention;
			Overflow = Math.Min(Math.Max(AbsoluteCapacity / 2, 2), 50);

			Items = new ConcurrentDictionary<KeyT, CacheItem<KeyT, ValueT>>(Environment.ProcessorCount, MinCache);

			PruneTimer = new System.Timers.Timer(10_000);
			if (interval.HasValue)
			{
				PruneTimer.Interval = interval.Value.TotalMilliseconds;
				PruneTimer.Elapsed += Prune;
				PruneTimer.Start();
			}
		}

		int prune_in_progress = 0;

		/// <summary>
		/// Prune cache.
		/// </summary>
		void Prune(object _discard, System.Timers.ElapsedEventArgs _ea)
		{
			if (disposed) return; // dumbness with timers
			if (!Atomic.Lock(ref prune_in_progress)) return; // only one instance.

			try
			{
				if (Items.Count <= MinCache) return; // just don't bother

				if (Items.Count <= AbsoluteCapacity && CacheEvictStrategy == EvictStrategy.LeastUsed) return;

				var list = Items.Values.ToList(); // would be nice to cache this list

				list.Sort((CacheItem<KeyT, ValueT> x, CacheItem<KeyT, ValueT> y) =>
				{
					if (CacheEvictStrategy == EvictStrategy.LeastRecent)
					{
						if (x.Access < y.Access) return -1;
						if (x.Access > y.Access) return 1;
						// Both have equal at this point
						if (x.Desirability < y.Desirability) return -1;
						if (x.Desirability > y.Desirability) return 1;
					}
					else // EvictStrategy.LeastUsed
					{
						if (x.Desirability < y.Desirability) return -1;
						if (x.Desirability > y.Desirability) return 1;
						// Both have equal at this point
						if (x.Access < y.Access) return -1;
						if (x.Access > y.Access) return 1;
					}

					return 0;
				});

				while (Items.Count > AbsoluteCapacity)
				{
					var bu = list[0];
					var key = bu.AccessKey;
					Items.TryRemove(key, out _);
					list.Remove(bu);
				}

				var deleteItem = new Action<KeyT>(
					(KeyT key) =>
					{
						//Debug.WriteLine($"MKAh.SimpleCache removing {bi:N1} min old item.");
						if (Items.TryRemove(key, out var item))
							list.Remove(item);
					}
				);

				var now = DateTimeOffset.UtcNow;
				while (list.Count > 0)
				{
					var bu = list[0];
					double bi = now.Since(bu.Access).TotalMinutes;

					if (CacheEvictStrategy == EvictStrategy.LeastRecent)
					{
						if (bi > MaxAge)
							deleteItem(bu.AccessKey);
						else
							break;
					}
					else // .LeastUsed
					{
						if (bi > MinAge)
							deleteItem(bu.AccessKey);
						else
							break;
					}
				}
			}
			finally
			{
				Atomic.Unlock(ref prune_in_progress);
			}
		}

		/// <summary>
		/// Add cache entry.
		/// </summary>
		/// <returns>The add.</returns>
		/// <param name="accesskey">Accesskey.</param>
		/// <param name="item">Item.</param>
		/// <param name="returntestkey">Returnkey.</param>
		public bool Add(KeyT accesskey, ValueT item)
		{
			//Misses++; // already counted for failed Get presumably

			if (Items.TryGetValue(accesskey, out var cci))
			{
				switch (CacheStoreStrategy)
				{
					case StoreStrategy.ReplaceNoMatch:
						if (cci.Item != item)
							goto replace;
						return false;
					case StoreStrategy.ReplaceAlways:
						replace:
						cci.Replace(item);
						break;
					case StoreStrategy.Fail:
						return false;
				}

				return true;
			}
			else
			{
				if (!Items.TryAdd(accesskey, new CacheItem<KeyT, ValueT>(accesskey, item)))
					return false; // shouldn't happen, but...
				return true;
			}
		}

		/// <summary>
		/// Get cached entry.
		/// </summary>
		/// <returns>True if value was found.</returns>
		/// <param name="key">Key.</param>
		/// <param name="item">Cached item.</param>
		/// <param name="testvalue">RT is tested for matching. Cache item is dropped if they don't match. Ignored if null.</param>
		public bool Get(KeyT key, out ValueT item)
		{
			try
			{
				if (Items.TryGetValue(key, out var citem))
				{
					citem.Desirability++;
					citem.Access = DateTimeOffset.UtcNow;
					item = citem.Item;
					Hits++;
					return true;
				}
			}
			catch (OutOfMemoryException) { throw; }
			catch
			{
				// NOP, don't caree
			}

			// this is bad design
			if (Count > AbsoluteCapacity + Overflow)
			{
				// TODO: make restart timer or something?
				/*
				 * Prune();
				 */
			}

			Misses++;
			item = default;
			return false;
		}

		/// <summary>
		/// Fully empty the cache.
		/// </summary>
		public void Empty() => Items.Clear();

		public void Clear() => Empty();

		public void Drop(KeyT key) => Items.TryRemove(key, out _);

		#region IDisposable Support
		bool disposed = false; // To detect redundant calls

		void Dispose(bool disposing)
		{
			if (disposed) return;
			disposed = true;

			if (disposing)
			{
				PruneTimer.Dispose();
				Items.Clear();

				//base.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}