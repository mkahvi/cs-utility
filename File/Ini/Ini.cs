﻿//
// Ini.cs
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

namespace MKAh
{
	namespace Ini
	{
		public class Config : Interface.IContainer<Section>, IEnumerable<Section>
		{
			public static readonly char[] ReservedCharacters
				= new[] { Constant.Quote, Constant.StandardComment, Constant.AlternateComment, Constant.KeyValueSeparator,
					Constant.ArrayStart, Constant.ArrayEnd, Constant.SectionStart, Constant.SectionEnd };

			/// <summary>
			///"   key=value # padding"
			/// </summary>
			//public int KeyAlign = 0;
			/// <summary>
			/// "key              =value # comment"
			/// </summary>
			//public int ValueAlign = 0;
			/// <summary>
			/// "key=value        # padded comment"
			/// </summary>
			//public int CommentAlign = 0;

			/// <summary>
			/// If true "key =value", if false "key=value"
			/// </summary>
			//public bool KeyPadding = true;
			/// <summary>
			/// If true "Key= Value #comment", if false "Key=Value#comment".
			/// </summary>
			//public bool ValuePadding = true;
			/// <summary>
			/// If true "key=value# comment", if false "key=value#comment"
			/// </summary>
			//public bool CommentPadding = true;

			/// <summary>
			/// Remove empty lines.
			/// </summary>
			const bool StripEmptyLines = true; // disabling not supported

			/// <summary>
			/// Add empty line above each section.
			/// </summary>
			public bool PadSections = true;

			public bool PreserveWhitespace = false;

			public bool AllowNamelessSections = false;

			public bool UniqueSections = false;
			public bool UniqueKeys { get; } = false; // setting this to true not supported yet

			public bool IgnoreMalformed = false;

			public bool AlwaysQuoteStrings = false;
			public bool RequireStringQuotes = false;

			public char CommentChar = Constant.StandardComment;
			public char[] CommentChars = { Constant.StandardComment, Constant.AlternateComment };

			public List<Section> Items { get; private set; } = new List<Section>();
			public int ItemCount => Items.Count;

			public string LineEnd = "\n";

			//HashSet<string> UniqueSectionNames = new HashSet<string>();

			public Config()
			{

			}

			public static Config FromData(string[] lines)
			{
				var config = new Config();
				config.Load(lines);
				return config;
			}

			public static Config FromFile(string filename) => FromFile(filename, System.Text.Encoding.UTF8);

			public static Config FromFile(string filename, System.Text.Encoding encoding)
			{
				var config = new Config();
				config.Load(filename, encoding);
				return config;
			}

			public void Load(string filename, System.Text.Encoding encoding)
			{
				Debug.WriteLine("INI LOAD: " + filename);

				using (var file = System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
				using (var reader = new System.IO.StreamReader(file, encoding))
				{
					Load(reader);
				}
			}

			public Section Header { get; private set; } = new Section("HEADER", -1);

			public void Load(string[] lines)
			{
				int lineNo = 0;

				Section section = Header;

				foreach (var line in lines)
				{
					lineNo++;
					HandleLine(line.Trim(), lineNo, ref section);
				}
			}

			public void Load(System.IO.StreamReader stream)
			{
				string line;
				int lineNo = 0;

				Section section = Header;

				while (!stream.EndOfStream)
				{
					lineNo++;

					// read line and trim whitespace from start
					line = stream.ReadLine().TrimStart();

					HandleLine(line, lineNo, ref section);
				}
			}

			void HandleLine(string line, int lineNumber, ref Section section)
			{
				Debug.WriteLine($"INI [{lineNumber:000}]: {line}");

				if (string.IsNullOrWhiteSpace(line))
				{
					if (StripEmptyLines) return;
					else section.Add(new Ini.Setting(SettingType.Empty));
				}

				if (line[0].Equals(Constant.SectionStart)) // section start
				{
					int SectionEnd = line.IndexOf(Constant.SectionEnd, 1);
					if (SectionEnd == -1)
					{
						if (IgnoreMalformed) return;
						throw new FormatException($"line:{lineNumber} has malformed section start: " + line.TrimEnd());
					}
					string SectionName = line.Substring(1, SectionEnd - 1);

					if (SectionName.IndexOfAny(ReservedCharacters) >= 0)
					{
						if (IgnoreMalformed) return;
						throw new FormatException($"line:{lineNumber} has invalid characters.");
					}

					if (!PreserveWhitespace) SectionName = SectionName.Trim();

					section = new Section(SectionName);
					Items.Add(section);
				}
				else
				{
					Setting value = null;
					try
					{
						value = ParseValue(line);
						section.Add(value);
					}
					catch (FormatException ex)
					{
						if (IgnoreMalformed) return;
						Debug.WriteLine("Malformed line:" + lineNumber + " - " + ex.Message);
						throw;
					}
					catch (Exception ex)
					{
						Debug.WriteLine(lineNumber + ": " + ex.Message);
						throw;
					}
				}
			}

			#region Indexer
			public Section this[int index]
			{
				get => Items[index];
				set => Insert(index, value);
			}

			public Section this[string name]
			{
				get
				{
					Section section = null;
					if (!TryGet(name, out section))
					{
						section = new Section(name);
						Items.Add(section);
					}

					return section;
				}
				set
				{
					value.UniqueKeys = UniqueKeys;

					// TODO: different behaviour with unique keys

					if (TryGet(value.Name, out var section))
					{
						Insert(Items.IndexOf(section), value);
						if (UniqueSections) Remove(section);
					}
					else
						Items.Add(value);
				}
			}
			#endregion

			public bool TryRemove(string name) => TryGet(name, out var section) ? Remove(section) : false;

			public bool Remove(Section section) => Items.Remove(section);
			public void RemoveAt(int index) => Items.RemoveAt(index);

			public void Add(Section section)
			{
				section.UniqueKeys = UniqueKeys;
				Items.Add(section);
			}

			public void Insert(int index, Section section) => Items.Insert(index, section);

			public Section Get(string name) => TryGet(name, out var section) ? section : null;

			public bool Contains(string name) => TryGet(name, out _);

			public bool TryGet(string name, out Section value)
				=> (value = (from val in Items
						 where !string.IsNullOrEmpty(val.Name)
						 where val.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
						 select val).FirstOrDefault()) != null;

			/// <summary>
			/// Parse .INI line other than section.
			/// </summary>
			Setting ParseValue(string source)
			{
				int CommentStart = source.IndexOfAny(CommentChars);
				int KeyValueSeparator = source.IndexOf(Constant.KeyValueSeparator);

				Setting value = null;

				if (KeyValueSeparator >= 0 && (CommentStart > KeyValueSeparator || CommentStart == -1))
				{
					value = new Setting() { Name = source.Substring(0, KeyValueSeparator).Trim() };

					KeyValueSeparator++; // skip =

					CommentStart = source.IndexOfAny(CommentChars, KeyValueSeparator);

					//	value = GetValue(source, Offset, CommentStart, out Offset); // try

					int ArrayStart = source.IndexOf(Constant.ArrayStart, KeyValueSeparator);

					int QuoteStart = source.IndexOf(Constant.Quote, KeyValueSeparator);

					int start = QuoteStart;

					bool CommentFound = CommentStart >= 0;
					bool QuoteFound = QuoteStart >= 0 && !source[QuoteStart - 1].Equals(Constant.EscapeChar);
					bool ArrayFound = ArrayStart >= 0;
					bool QuoteAndArray = QuoteFound && ArrayFound;
					bool QuoteOrArray = QuoteFound || ArrayFound;

					int end = KeyValueSeparator;

					if (CommentFound && CommentStart < Math.Min(QuoteStart.Replace(-1, int.MaxValue), ArrayStart.Replace(-1, int.MaxValue)))
					{
						// comment found before anything else

						end = CommentStart;
						value.Value = source.Substring(KeyValueSeparator, CommentStart - KeyValueSeparator);
					}
					else if (ArrayFound && (ArrayStart < QuoteStart || QuoteStart == -1))
					{
						// proper array
						// { "" }

						//Debug.WriteLine("GetArray: " + source);

						var array = GetArray(source, ArrayStart, out end);
						value.Array = value.UnescapeArray(array);

						//Debug.WriteLine("Escaped:  "+value.EscapedValue);

						//value.Value = source.Substring(ArrayStart, end - ArrayStart).Trim();

						// TODO: check for garbage before and after array delimiters
					}
					else if (QuoteFound && !source[QuoteStart - 1].Equals(Constant.EscapeChar))
					{
						// properly quoted string
						// "

						start = QuoteStart;

						value.Value = GetQuotedString(source, QuoteStart, out end);

						// TODO: check for garbage before and after quote delimiters
					}
					else // basic unquoted string
					{
						// no comment, no quotes

						end = source.Length - 1;
						value.Value = source.Substring(KeyValueSeparator);
					}

					//Debug.WriteLine($"STRING SEARCH [{start}-{end}] = \"{source}\" -> \"{rv}\"");

					if (CommentStart < end) CommentStart = source.IndexOfAny(CommentChars, end);

					if (CommentStart >= 0) value.Comment = source.Substring(CommentStart + 1).Trim();

					if (!PreserveWhitespace && !string.IsNullOrEmpty(value.Value)) value.Value = value.Value.Trim();
				}
				else // no key=value
				{
					//Offset++;

					if (CommentStart == -1) throw new FormatException("Malformed line: no key=value pair nor comment\n" + source);

					value = new Setting { Comment = source.Substring(CommentStart + 1).Trim() };
				}

				if (!string.IsNullOrEmpty(value.Value) && value.UnescapeValue(value.Value, out string nv))
					value.Value = nv;

				return value;
			}

			string[] GetArray(string source, int offset, out int end)
			{
				Debug.Assert(source[offset].Equals('{'));
				offset++; // skip {

				var rv = new List<string>();

				bool expectArrayDelimiter = false;

				int itemStart = offset;

				int Quotes = 0;

				bool UnescapedQuote = false;
				bool ArrayDelimiter = false;
				bool ArrayEnd = false;

				for (int i = offset; i < source.Length; i++)
				{
					UnescapedQuote = source[i].Equals(Constant.Quote) && !source[i - 1].Equals(Constant.EscapeChar);

					if (Quotes % 2 != 0) // inside quotes
					{
						if (UnescapedQuote)
						{
							Quotes++;
							expectArrayDelimiter = true;
						}
						continue;
					}
					else // outside of quotes
					{
						if (UnescapedQuote)
						{
							Quotes++;
							continue;
						}
					}

					if (char.IsWhiteSpace(source[i])) continue; // always skip whitespace
					ArrayEnd = source[i].Equals(Constant.ArrayEnd);
					ArrayDelimiter = source[i].Equals(Constant.ArrayDelimiter);

					if (ArrayEnd || ArrayDelimiter) // array split or end
					{
						string item = source.Substring(itemStart, i - itemStart);
						rv.Add(item);

						if (ArrayEnd)
						{
							end = i + 1;
							return rv.ToArray();// anything else is broken formatting
						}
						else
						{
							expectArrayDelimiter = false;
							itemStart = i + 1;
						}
					}
					else if (expectArrayDelimiter && !ArrayDelimiter)
						throw new FormatException($"Array item delimiter expected at {i}, found {source[i]} instead");

					if (CommentChars.Contains(source[i])) // comment char outside of quotes
						throw new FormatException("Unexpected comment start before array closure [" + i + "]: " + source);
				}

				throw new FormatException("Array end not found.");
			}

			string GetQuotedString(string source, int offset, out int end)
			{
				Debug.Assert(source[offset].Equals(Constant.Quote));

				offset++; // skip "

				bool UnescapedQuote = false;
				for (int i = offset; i < source.Length; i++)
				{
					UnescapedQuote = source[i].Equals(Constant.Quote) && !source[i - 1].Equals(Constant.EscapeChar);
					if (UnescapedQuote)
					{
						end = i + 1;
						var final = source.Substring(offset, i - offset);
						return final;
					}
				}

				throw new FormatException("Quoted string end not found.");
			}

			public IEnumerable<string> EnumerateLines()
			{
				int lineNo = 0;

				if (Header.ItemCount > 0)
				{
					foreach (Setting item in Header)
					{
						item.Line = lineNo++;
						yield return item.Type != SettingType.Empty ? item.ToString() : string.Empty;
					}

					if (PadSections)
					{
						lineNo++;
						yield return string.Empty;
					}
				}

				foreach (Section section in this)
				{
					section.Line = lineNo++;
					yield return $"{Constant.SectionStart}{section.Name}{Constant.SectionEnd}";

					foreach (Setting item in section)
					{
						item.Line = lineNo++;
						yield return item.Type != SettingType.Empty ? item.ToString() : string.Empty;
					}

					if (PadSections)
					{
						lineNo++;
						yield return string.Empty;
					}
				}
			}

			public string[] GetLines()
			{
				int totallines = ItemCount;

				if (PadSections)
				{
					totallines *= 2;
					totallines--;
				}

				if (Header.ItemCount > 0) totallines += Header.ItemCount + (PadSections ? 1 : 0);

				foreach (var section in this)
					totallines += section.Items.Count;

				Debug.WriteLine("INI GET LINES: " + totallines);

				if (totallines == 0) throw new ArgumentNullException("Empty configuration");

				var output = new List<string>(totallines);

				string formatted = null;

				int LineNo = 0;

				if (Header.ItemCount > 0)
				{
					foreach (var kval in Header.Items)
					{
						switch (kval.Type)
						{
							default:
								formatted = kval.ToString() + LineEnd;
								output.Add(formatted);
								kval.Line = LineNo++;
								break;
							case SettingType.Empty:
								if (!StripEmptyLines)
								{
									output.Add(LineEnd);
									kval.Line = LineNo++;
								}
								break;
						}
					}
					if (PadSections)
					{
						output.Add(LineEnd);
						LineNo++;
					}
				}

				foreach (var section in Items)
				{
					if (section.Index >= 0)
					{
						formatted = $"[{section.Name}]{LineEnd}";
						output.Add(formatted);
						section.Line = LineNo++;
					}

					foreach (var kval in section.Items)
					{
						switch (kval.Type)
						{
							default:
								formatted = kval.ToString() + LineEnd;
								output.Add(formatted);
								kval.Line = LineNo++;
								break;
							case SettingType.Empty:
								if (!StripEmptyLines)
								{
									output.Add(LineEnd);
									kval.Line = LineNo++;
								}
								break;
						}
					}

					if (PadSections)
					{
						output.Add(LineEnd);
						LineNo++;
					}
				}

				return output.ToArray();
			}

			public void SaveToFile(string filename) => SaveToFile(filename, System.Text.Encoding.UTF8);

			public void SaveToFile(string filename, System.Text.Encoding encoding)
			{
				var lines = GetLines();

				using (var file = System.IO.File.Open(filename, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.None))
				using (var writer = new System.IO.StreamWriter(file, encoding, 1024 * 1024 * 64))
				{
					file.SetLength(0); // dumb, but StreamWriter is unhelpful

					SaveToStream(writer, lines);
				}
				lines = null;
			}

			public void SaveToStream(System.IO.StreamWriter writer, string[] lines = null)
			{
				if (lines == null)
					lines = GetLines();

				foreach (var line in lines)
					writer.Write(line);
			}

			public IEnumerator<Section> GetEnumerator() => Items.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
