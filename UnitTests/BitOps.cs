using System;
using MKAh;
using NUnit;
using NUnit.Framework;

namespace Logic
{
	[TestFixture]
	public class Bit
	{
		[Test]
		[TestOf(nameof(MKAh.Logic.Bit.Move))]
		[TestCase(0b0100, 0b0010, ExpectedResult = 0b0010)]
		public int Move(int source, int target)
		{
			int result = MKAh.Logic.Bit.Move(source, target);

			Assert.AreEqual(target, result, "{0} does not match {1}", Convert.ToString(result, 2).PadLeft(4), Convert.ToString(target, 2).PadLeft(4));

			return result;
		}

		[Test]
		[TestOf(nameof(MKAh.Logic.Bit.Unfill))]
		[TestCase(0b1110, 0b0010, 1, ExpectedResult = 0b1010)]
		[TestCase(0b1110, 0b0010, 2, ExpectedResult = 0b0010)]
		public int Unfill(int source, int allowed, int max)
		{
			return MKAh.Logic.Bit.Unfill(source, allowed, max);
		}
	}
}
