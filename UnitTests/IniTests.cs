using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace MKAh.Ini
{
	class ExposedSetting : Setting
	{
		public string ExposedEscapedValue => base.EscapedValue;

		internal ExposedSetting() : base()
		{

		}

		internal ExposedSetting(Setting setting) : base()
		{
			Comment = setting.Comment;
			if (setting.IsArray)
				Array = setting.Array;
			else
				Value = setting.Value;
			Name = setting.Name;
		}
	}
}

namespace IniFile
{
	using MKAh;
	using MKAh.Ini;
	using Ini = MKAh.Ini;

	[TestFixture]
	public class Reading
	{
		[Test]
		[Repeat(3)]
		public void Initialization()
		{
			var config = new Ini.Config();

			var section = new Ini.Section("Test");

			config.Add(section);

			section.Add(new Ini.Setting() { Name = "Key" }); // empty value

			var lines = config.GetLines();

			foreach (var line in lines)
				Debug.WriteLine(line);

			Assert.AreEqual(3, lines.Length);
			Assert.AreEqual("[Test]\n", lines[0]);

			Assert.AreEqual(2, config.Changes);
		}

		[Test]
		public void LoadFromText()
		{
			var data = UnitTests.Properties.Resources.MixedTest;

			var config = new Ini.Config();

			config.Load(data.Split('\n'));

			Assert.AreEqual(0, config.Changes);
		}

		[Test]
		public void StoreValue()
		{
			try
			{
				var section = new Ini.Section("Test");

				var key0 = new Ini.Setting { Name = "Key0", Value = "#\"bob\"", Comment = "string" };
				Debug.WriteLine(key0.Value);
				Assert.AreEqual("#\"bob\"", key0.Value);

				var key1 = new Ini.Setting { Name = "Key1", Comment = "float" };
				key1.Set(0.5f);
				Debug.WriteLine(key1.Value);
				Assert.AreEqual("0.5", key1.Value);

				var intarray = new Ini.ExposedSetting { Name = "IntArray", Comment = "ints" };
				intarray.SetArray(new[] { 1, 2f, 3 });
				Debug.WriteLine(intarray.ExposedEscapedValue);
				Assert.AreEqual("{ 1, 2, 3 }", intarray.ExposedEscapedValue);

				var strarray = new Ini.ExposedSetting { Name = "StringArray", Comment = "strings" };
				strarray.SetArray(new[] { "abc", "xyz" });
				Debug.WriteLine(strarray.ExposedEscapedValue);
				Assert.AreEqual("{ abc, xyz }", strarray.ExposedEscapedValue);

				var badarray = new Ini.ExposedSetting { Name = "BadArray", Comment = "bad strings" };
				badarray.SetArray(new[] { "a#b#c", "x\"y\"z", "\"doop\"#", "good", "  spaced", "#bad", "#\"test\"" });
				Debug.WriteLine(badarray.ExposedEscapedValue);

				Assert.AreEqual("{ \"a#b#c\", \"x\\\"y\\\"z\", \"\\\"doop\\\"#\", good, \"  spaced\", \"#bad\", \"#\\\"test\\\"\" }", badarray.ExposedEscapedValue);

				var quotedArray = new Ini.Setting { Name = "test", Value = "kakka\"bob\"" };

				section.Add(key0);
				section.Add(key1);
				section.Add(intarray);
				section.Add(strarray);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void Indexers()
		{
			var config = new Ini.Config();

			config["Section"]["Key"].Value = "HaHaHa";

			Assert.AreEqual(1, config.ItemCount);
			Assert.AreEqual(1, config.Items.First().ItemCount);

			var section = config["Section"];

			Assert.AreEqual(1, section.ItemCount);

			var value = section["Key"];

			Assert.AreEqual("HaHaHa", value.Value);

			Assert.AreEqual(3, config.Changes);
		}

		public void EmptyValues()
		{
			// TODO
		}

		[Test]
		public void SectionEnumerator()
		{
			var config = new Ini.Config();

			config.Add(new Ini.Section("Test1", 5));
			config.Add(new Ini.Section("Test2", 72));

			var results = new List<string>();
			var resultsInt = new List<int>();

			foreach (Ini.Section section in config)
			{
				results.Add(section.Name);
				resultsInt.Add(section.Index);
			}

			Assert.AreEqual("Test1", results[0]);
			Assert.AreEqual(5, resultsInt[0]);
			Assert.AreEqual("Test2", results[1]);
			Assert.AreEqual(72, resultsInt[1]);
		}

		[Test]
		public void SettingEnumerator()
		{
			var section = new Ini.Section("TestSection");

			section.Add(new Ini.Setting() { Name = "Test1", Int = 5 });
			section.Add(new Ini.Setting() { Name = "Test2", Int = 72 });

			var results = new List<string>();
			var resultInts = new List<string>();

			foreach (Ini.Setting setting in section)
			{
				results.Add(setting.Name);
				resultInts.Add(setting.Value);
			}

			Assert.AreEqual("Test1", results[0]);
			Assert.AreEqual("5", resultInts[0]);
			Assert.AreEqual("Test2", results[1]);
			Assert.AreEqual("72", resultInts[1]);
		}

		[Test]
		[TestCase(3)]
		public void RecursiveSaveAndLoad(int repeats)
		{
			if (repeats < 2) repeats = 2;
			if (repeats > 30) repeats = 30;

			var config = new Ini.Config();

			var section1Name = "Number Tests";
			var section2Name = "String Tests";
			var section1Var1Name = "IntArray";
			var section1Var2Name = "DoubleValue";
			var section2Var1Name = "StringArray";
			var section2Var2Name = "String";

			var s1var1value = new int[] { 1, 2, 3 };
			var s1var1valueEscaped = "{ 1, 2, 3 }";
			var s1var2value = 0.5d;
			var s1var2valueFormatted = "0.5";
			var s2var1value = new string[] { "abc", "xyz" };
			var s2var1valueEscaped = "{ abc, xyz }";
			var s2var2value = "Bad\"#";
			var s2var2valueEscaped = "\"Bad\\\"#\"";

			var section1 = new Ini.Section(section1Name);
			section1.Add(new Ini.Setting() { Name = section1Var1Name, IntArray = s1var1value });
			section1.Add(new Ini.Setting() { Name = section1Var2Name, Double = s1var2value });
			config.Add(section1);

			var section2 = new Ini.Section(section2Name);
			section2.Add(new Ini.Setting() { Name = section2Var1Name, Array = s2var1value });
			section2.Add(new Ini.Setting() { Name = section2Var2Name, Value = s2var2value });
			config.Add(section2);


			const string incrName = "Incrementor";
			const string rollName = "Roller";

			var section3 = new Section(incrName);
			section3.Add(new Setting() { Name = rollName, Int = 0 });
			config.Add(section3);

			var data = config.GetLines();

			for (int i = 0; i < repeats; i++)
			{
				config = Ini.Config.FromData(data); // read written config

				Assert.AreEqual(3, config.ItemCount);
				var s1 = config.Get(section1Name);
				var s2 = config.Get(section2Name);

				Assert.IsNotNull(s1);
				Assert.IsNotNull(s2);

				var s1v1 = s1.Get(section1Var1Name);
				var s1v2 = s1.Get(section1Var2Name);
				var s2v1 = s2.Get(section2Var1Name);
				var s2v2 = s2.Get(section2Var2Name);

				Assert.IsNotNull(s1v1);
				Assert.IsNotNull(s1v2);
				Assert.IsNotNull(s2v1);
				Assert.IsNotNull(s2v2);

				Assert.AreEqual(s1var1valueEscaped, new ExposedSetting(s1v1).ExposedEscapedValue);
				Assert.AreEqual(s1var2valueFormatted, s1v2.Value);
				Assert.AreEqual(s2var1valueEscaped, new ExposedSetting(s2v1).ExposedEscapedValue);
				Assert.AreEqual(s2var2valueEscaped, new ExposedSetting(s2v2).ExposedEscapedValue);

				int num = config[incrName][rollName].Int;
				Assert.AreEqual(i, num);
				config[incrName][rollName].Int = num + 1;

				data = config.GetLines(); // re-write config
			}
		}

		[Test]
		public void GetSetDefaultDoesSet()
		{
			var config = new Ini.Config();
			var section = new Ini.Section("Test");
			config.Add(section);

			int newVal = 5;

			int mods = config.Changes;
			var setVal = section.GetOrSet("NotSet", newVal);

			Assert.AreEqual(mods + 2, config.Changes);
			Assert.AreEqual(newVal, setVal.Int);

			var setVal2 = section.GetOrSet("NotSetArray", new int[] { 1, 2, 3 });
			Assert.AreEqual(mods + 4, config.Changes);

			var array = setVal2.IntArray;
			Assert.AreEqual(1, array[0]);
			Assert.AreEqual(2, array[1]);
			Assert.AreEqual(3, array[2]);
			Assert.AreEqual("{ 1, 2, 3 }", new ExposedSetting(setVal2).ExposedEscapedValue);

			Assert.AreEqual(5, config.Changes);
		}

		[Test]
		public void GetSetDefaultGetOnly()
		{
			var config = new Ini.Config();
			var section = new Ini.Section("Test");
			config.Add(section);

			string SettingName = "Preset";
			string SettingName2 = "ArrayPreset";

			int oldVal = 5;
			int newVal = 7;

			section[SettingName].Int = oldVal;

			int mods = config.Changes;
			var setting = section.GetOrSet(SettingName, newVal);

			Assert.AreEqual(mods, config.Changes);
			Assert.AreEqual(oldVal, setting.Int);

			section[SettingName2].IntArray = new int[] { 1, 2, 3 };

			mods = config.Changes;
			var setting2 = section.GetOrSet(SettingName2, new int[] { 7, 8, 9 });

			Assert.AreEqual(mods, config.Changes); // no changes
			var array = setting2.IntArray;
			Assert.AreEqual(1, array[0]);
			Assert.AreEqual(2, array[1]);
			Assert.AreEqual(3, array[2]);
			Assert.AreEqual("{ 1, 2, 3 }", new ExposedSetting(setting2).ExposedEscapedValue);
		}

		[Test]
		[TestOf(nameof(Ini.Interface.Value.Name))]
		public void NameChange()
		{
			var config = new Ini.Config();
			var section = new Ini.Section("Test");
			config.Add(section);

			var setting = new Ini.Setting() { Name = "Name" };
			section.Add(setting);

			int mods = config.Changes;
			section.Name = "tseT";
			Assert.AreEqual(mods + 1, config.Changes);

			setting.Name = "emaN";
			Assert.AreEqual(mods + 2, config.Changes);
		}

		[Test]
		[TestOf(nameof(Ini.Setting.Array))]
		public void EmptyArray()
		{
			var section = new Ini.Section("Test");

			string intSettingName = "IntArray";
			string stringSettingName = "StringArray";
			string nullSettingName = "NullArray";

			section[intSettingName].IntArray = new int[] { };
			section[stringSettingName].Array = new string[] { };
			section[nullSettingName].Array = null;

			var nullArray = section[nullSettingName].Array;
			var stringArray = section[stringSettingName].StringArray;
			var intArray = section[intSettingName].IntArray;

			Assert.AreEqual(null, nullArray?.Length ?? null);
			Assert.AreEqual(0, intArray.Length);
			Assert.AreEqual(0, stringArray.Length);
		}

		[Test]
		public void PreserveEmptyLines()
		{
			var data = UnitTests.Properties.Resources.EmptyLines;

			var config = new Ini.Config();

			config.StripEmptyLines = false;

			var datalines = data.Split(new[] { '\n' }, StringSplitOptions.None);

			config.Load(datalines);

			var newlines = config.GetLines();

			foreach (var line in newlines)
				Debug.Write(line);
			Debug.WriteLine("");

			Assert.AreEqual(datalines.Length, newlines.Length);
		}

		/// <summary>
		/// Tests if comments leak into actual variables.
		/// </summary>
		[Test]
		public void CommentTainting()
		{
			string testsite = @"Taint = 5 # 0 = I, 1 = D, 2 = V, 3 = E; 2";
			string full = "[Test]\n" + testsite + "\n\n";

			var config = new Ini.Config();
			config.Load(full.Split(new[] { '\n' }, StringSplitOptions.None));

			Assert.AreEqual(5, config.Get("Test").Get("Taint").Int);
		}
	}
}
