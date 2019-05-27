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
using System.Diagnostics;
using System.Linq;

namespace MKAh.Cache
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="KT">Access key.</typeparam>
	/// <typeparam name="RT">Secondary value, returned with the value.</typeparam>
	/// <typeparam name="VT">Value.</typeparam>
	public class SimpleCache<KT, RT, VT> : IDisposable where VT : class where RT : class
	{
		readonly EvictStrategy CacheEvictStrategy = EvictStrategy.LeastRecent;
		readonly StoreStrategy CacheStoreStrategy = StoreStrategy.ReplaceNoMatch;

		readonly ConcurrentDictionary<KT, CacheItem<KT, RT, VT>> Items = new ConcurrentDictionary<KT, CacheItem<KT, RT, VT>>();

		public ulong Count => Convert.ToUInt64(Items.Count);
		public ulong Hits { get; private set; } = 0;
		public ulong Misses { get; private set; } = 0;

		readonly System.Timers.Timer PruneTimer = null;

		uint Overflow = 10;
		uint MaxCache = 20;
		uint MinCache = 10;

		uint MinAge = 5;
		uint MaxAge = 60;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="maxItems">Attempt to have no more than this many items.</param>
		/// <param name="minItems">Do not prune if this many or less items.</param>
		/// <param name="interval">Automatic pruning interval.</param>
		/// <param name="store"></param>
		/// <param name="evict"></param>
		public SimpleCache(uint maxItems = 100, uint minItems = 10, TimeSpan? interval = null,
					 StoreStrategy store = StoreStrategy.ReplaceNoMatch, EvictStrategy evict = EvictStrategy.LeastRecent)
		{
			CacheStoreStrategy = store;
			CacheEvictStrategy = evict;

			MaxCache = maxItems;
			MinCache = minItems;
			Overflow = Math.Min(Math.Max(MaxCache / 2, 2), 50);

			if (interval.HasValue)
			{
				if (interval.Value.TotalSeconds < 10) interval = new TimeSpan(0, 0, 10);
				PruneTimer = new System.Timers.Timer(interval.Value.TotalMilliseconds);
				PruneTimer.Elapsed += Prune;
				PruneTimer.Start();
			}
		}

		int prune_in_progress = 0;

		/// <summary>
		/// Prune cache.
		/// </summary>
		void Prune(object _discard, EventArgs _ea)
		{
			if (disposed) return; // dumbness with timers
			if (!Atomic.Lock(ref prune_in_progress)) return; // only one instance.

			try
			{
				if (Items.Count <= MinCache) return; // just don't bother

				if (Items.Count <= MaxCache && CacheEvictStrategy == EvictStrategy.LeastUsed) return;

				var list = Items.Values.ToList(); // would be nice to cache this list

				list.Sort(delegate (CacheItem<KT, RT, VT> x, CacheItem<KT, RT, VT> y)
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

				while (Items.Count > MaxCache)
				{
					var bu = list.ElementAt(0);
					var key = bu.AccessKey;
					Items.TryRemove(key, out _);
					list.Remove(bu);
				}

				double bi = double.NaN;

				var deleteItem = new Action<KT>(
					(KT key) =>
					{
						//Debug.WriteLine($"MKAh.SimpleCache removing {bi:N1} min old item.");
						if (Items.TryRemove(key, out var item))
							list.Remove(item);
					}
				);

				var now = DateTimeOffset.UtcNow;
				while (list.Count > 0)
				{
					var bu = list.ElementAt(0);
					bi = now.TimeSince(bu.Access).TotalMinutes;

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
		public bool Add(KT accesskey, VT item, RT returntestkey=null)
		{
			Misses++;

			if (Items.ContainsKey(accesskey))
			{
				if (CacheStoreStrategy == StoreStrategy.Fail)
					return false;

				Items.TryRemove(accesskey, out _); // .Replace
			}

			var ci = new CacheItem<KT, RT, VT> { AccessKey = accesskey, ReturnKey = returntestkey, Item = item, Access = DateTimeOffset.UtcNow, Desirability = 1 };
			CacheItem<KT, RT, VT> t = ci;
			Items.TryAdd(accesskey, t);

			return true;
		}

		/// <summary>
		/// Get cached entry.
		/// </summary>
		/// <returns>The get.</returns>
		/// <param name="key">Key.</param>
		/// <param name="item">Cacheditem.</param>
		/// <param name="testvalue">RT is tested for matching. Cache item is dropped if they don't match. Ignored if null.</param>
		public RT Get(KT key, out VT item, RT testvalue = null)
		{
			try
			{
				if (Items.TryGetValue(key, out var citem))
				{
					if (testvalue?.Equals(citem.ReturnKey) ?? false)
					{
						item = null;
						Misses++;
						Drop(key);
						return null;
					}

					citem.Desirability++;
					citem.Access = DateTimeOffset.UtcNow;
					item = citem.Item;
					Hits++;
					return citem.ReturnKey;
				}
			}
			catch (OutOfMemoryException) { throw; }
			catch
			{
				// NOP, don't caree
			}

			// this is bad design
			if (Count > MaxCache + Overflow)
			{
				// TODO: make restart timer or something?
				/*
				 * Prune();
				 */
			}

			Misses++;
			item = null;
			return null;
		}

		/// <summary>
		/// Fully empty the cache.
		/// </summary>
		public void Empty() => Items.Clear();

		public void Drop(KT key) =>  Items.TryRemove(key, out _);

		#region IDisposable Support
		bool disposed = false; // To detect redundant calls

		void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				PruneTimer?.Dispose();
				Items?.Clear();
			}

			disposed = true;
		}

		public void Dispose() => Dispose(true);
		#endregion
	}
}
