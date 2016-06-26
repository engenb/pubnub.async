using System;
using Ploeh.AutoFixture;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Models.Channel
{
	public class ChannelTests : AbstractTest
	{
		[Fact]
		public void ctor__Given_ChannelName__Then_SetName()
		{
			var expectedChannelName = Fixture.Create<string>();
			var subject = new Async.Models.Channel.Channel(expectedChannelName);

			Assert.Equal(expectedChannelName, subject.Name);
		}

		[Fact]
		public void ctor__Given_NullChannelName__Then_ThrowEx()
		{
			var ex = Assert.Throws<ArgumentException>(() => new Async.Models.Channel.Channel(null));
			Assert.Equal(ex.ParamName, "name");
		}

		[Fact]
		public void ctor__Given_WhitespaceChannelName__Then_ThrowEx()
		{
			var ex = Assert.Throws<ArgumentException>(() => new Async.Models.Channel.Channel(" "));
			Assert.Equal(ex.ParamName, "name");
		}
	}
}