using System;
using System.Runtime.InteropServices;
using MKAh;
using NUnit;
using NUnit.Framework;

namespace Timing
{
	[TestFixture]
	public class Time
	{
		[DllImport("kernel32.dll")]
		internal static extern uint GetTickCount();

		[Test]
		[TestOf(nameof(GetTickCount))]
		[Retry(3)]
		public void GetTickCountEquality()
		{
			uint eticks = (uint)Environment.TickCount;
			uint pticks = GetTickCount();

			Console.WriteLine("C# Env:   " + eticks.ToString());
			Console.WriteLine("P/Invoke: " + pticks.ToString());

			Assert.AreEqual(eticks/1000d, pticks/1000d, 5d);
		}

		[Test]
		[TestOf(nameof(MKAh.User.CorrectIdleTime))]
		[Retry(3)]
		public void GetTickCountCorrection()
		{
			Assert.AreEqual(10_000L, MKAh.User.CorrectIdleTime(12_000, 22_000), "Normal");
		}

		[Test]
		[TestOf(nameof(MKAh.User.CorrectIdleTime))]
		[Retry(3)]
		public void GetTickCountWrapAround()
		{
			const uint diff = 5_000;
			const uint lowTick = diff;
			const uint highTick = uint.MaxValue - diff;
			const long actualdiff = diff * 2L; // (uint.max - diff) to (0 + diff);

			long result = MKAh.User.CorrectIdleTime(highTick, lowTick);

			Console.WriteLine($"GetTickCount: {highTick} -> {lowTick} = {result}");

			Assert.AreEqual(actualdiff, result, "49 day wrap around failpoint");
		}
	}
}
