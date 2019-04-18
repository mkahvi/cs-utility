//
// Ini.Section.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MKAh.Ini
{
	public class Section : Interface.Value, Interface.IContainer<Setting>, IEnumerable<Setting>
	{
		public Section(string name, int index = 0)
		{
			Name = name;
			Index = index;
		}

		public List<Setting> Items { get; private set; } = new List<Setting>();
		public int ItemCount => Items.Count;

		HashSet<string> hUniqueKeys = null;

		public int Index { get; internal set; } = 0;

		bool _uniqueKeys = false;
		public bool UniqueKeys
		{
			get => _uniqueKeys;
			set => hUniqueKeys = (_uniqueKeys = value) ? new HashSet<string>() : null;
		}

		#region Indexers
		public Setting this[int index]
		{
			get => Items[index];
			set => Insert(index, value);
		}

		public Setting this[string key]
		{
			get
			{
				Setting value = null;
				if (!TryGet(key, out value))
					Items.Add(value = new Setting() { Name = key });

				return value;
			}
			set
			{
				// TODO: different behaviour with unique keys
				if (TryGet(key, out var result))
				{
					Insert(Items.IndexOf(result), value);
					if (UniqueKeys) Remove(result);
				}
				else
					Add(value);
			}
		}
		#endregion

		public Setting Get(string key) => TryGet(key, out var value) ? value : null;

		public bool TryGet(string name, out Setting value)
			=> (value = (from val in Items
						 where val.Type == SettingType.Generic
						 where !string.IsNullOrEmpty(val.Name)
						 where val.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
						 select val).FirstOrDefault()) != null;

		public bool Contains(string key) => TryGet(key, out _);

		public void Add(Setting value) => Items.Add(value);
		public void Insert(int index, Setting value) => Items.Insert(value.Index = index, value);
		public bool Remove(Setting value) => Items.Remove(value);
		public void RemoveAt(int index) => Items.RemoveAt(index);

		public bool TryRemove(string key)
		{
			if (TryGet(key, out var value))
			{
				Remove(value);
				return true;
			}

			return false;
		}

		public IEnumerator<Setting> GetEnumerator() => Items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
