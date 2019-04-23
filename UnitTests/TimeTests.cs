﻿using System;
using MKAh;
using NUnit;
using NUnit.Framework;

namespace Timing
{
	[TestFixture]
	public class Time
	{
		[Test]
		[TestOf(nameof(MKAh.Native.GetTickCount))]
		[Retry(3)]
		public void GetTickCountEquality()
		{
			uint eticks = (uint)Environment.TickCount;
			uint pticks = MKAh.Native.GetTickCount();

			Console.WriteLine("C# Env:   " + eticks);
			Console.WriteLine("P/Invoke: " + pticks);

			Assert.AreEqual(eticks/1000, pticks/1000);
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
			uint diff = 5_000;
			uint lowTick = diff;
			uint highTick = uint.MaxValue - diff;

			long result = MKAh.User.CorrectIdleTime(highTick, lowTick);

			Console.WriteLine($"GetTickCount: {highTick} -> {lowTick} = {result}");

			Assert.AreEqual(diff*2L, result, "49 day wrap around failpoint");
		}
	}
}
