using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;

namespace Types
{
	[TestFixture]
	public partial class Trinary
	{
		[Test]
		[TestOf(nameof(MKAh.Types.Trinary))]
		public void Equality()
		{
			var nonce = MKAh.Types.Trinary.Nonce;

			Assert.False(true == nonce);
			Assert.False(false == nonce);
			Assert.False(nonce == true);
			Assert.False(nonce == false);

			var ttrue = MKAh.Types.Trinary.True;
			var ttrue2 = MKAh.Types.Trinary.True;

			Assert.True(ttrue == true);
			Assert.True(true == ttrue);
			Assert.True(ttrue == ttrue2);

			var tfalse = MKAh.Types.Trinary.False;
			var tfalse2 = MKAh.Types.Trinary.False;

			Assert.True(tfalse == false);
			Assert.True(false == tfalse);
			Assert.True(tfalse == tfalse2);

			bool cfalse = true;
			bool ctrue = false;
			bool cnonce = false;

			var tnonce = MKAh.Types.Trinary.Nonce;

			Assert.DoesNotThrow(() => { cfalse = tfalse; });
			Assert.DoesNotThrow(() => { ctrue = ttrue; });
			//Assert.Throws<ArgumentException>(() => { cnonce = tnonce; });
		}
	}
}
