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
using System.Diagnostics;
using System.Linq;

namespace MKAh.Ini
{
	public class Section : Interface.Value, Interface.IContainer<Setting>, IEnumerable<Setting>
	{
		public Section(string name, int index = 0, Config? parent = null)
		{
			Name = name;
			Index = index;
			Parent = parent;
		}

		public Config? Parent { get; set; } = null;

		internal void ChildAltered(Setting value) => Parent?.ChildAltered(this, value);

		protected override void Altered(Interface.Value child) => ChildAltered(child as Setting);

		public List<Setting> Items { get; } = new List<Setting>();
		public int ItemCount => Items.Count;

		HashSet<string>? hUniqueKeys = null;

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
			set
			{
				Insert(index, value);
				Own(value);
			}
		}

		public Setting this[string key]
		{
			get
			{
				if (!TryGet(key, out Setting value))
					Add(value = new Setting() { Name = key, Parent = this });

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

				Own(value);
			}
		}
		#endregion

		public Setting Get(string key) => TryGet(key, out var value) ? value : null;

		public bool TryGet(string name, out Setting value)
			=> (value = Items.FirstOrDefault(x => x.Type == SettingType.Generic && x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))) != null;

		public bool Contains(string key) => TryGet(key, out _);

		void Own(Setting setting)
		{
			setting.Parent = this;
			ChildAltered(setting);
		}

		void Deown(Setting setting)
		{
			setting.Parent = null;
			ChildAltered(setting);
		}

		public void Add(Setting value)
		{
			Items.Add(value);
			Own(value);
		}

		public void Insert(int index, Setting value)
		{
			Items.Insert(value.Index = index, value);
			Own(value);
		}

		public bool TryRemove(string key)
		{
			if (TryGet(key, out var value))
			{
				Remove(value);
				return true;
			}

			return false;
		}

		public bool Remove(Setting value)
		{
			Deown(value);
			return Items.Remove(value);
		}

		public void RemoveAt(int index)
		{
			var setting = Items[index];
			Remove(setting);
		}

		public IEnumerator<Setting> GetEnumerator() => Items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public Setting GetOrSet<T>(string setting, T[] fallback)
		{
			if (!(TryGet(setting, out var rv) && rv.IsArray))
			{
				if (rv is null) rv = new Setting() { Name = setting, Parent = this };

				rv.SetArray(Converter<T>.Convert(fallback));
				Add(rv);
			}

			return rv;
		}

		/// <summary>
		/// .
		/// </summary>
		/// <remarks>Causes 0 to 2 changes.</remarks>
		public Setting GetOrSet<T>(string setting, T fallback)
		{
			Debug.Assert(!string.IsNullOrEmpty(setting));

			if (!(TryGet(setting, out var rv) && rv.Value != null))
			{
				if (rv is null) rv = new Setting() { Name = setting, Parent = this };

				rv.Set<T>(fallback);
				//rv.Set(Converter<T>.Convert(fallback));

				Add(rv);
			}

			return rv;
		}
	}
}
