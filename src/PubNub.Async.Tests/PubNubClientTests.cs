using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubNub.Async.Models.Channel;
using Xunit;

namespace PubNub.Async.Tests
{
	public class PubNubClientTests
	{
		[Fact]
		public void Secured__Then_SecureChannel()
		{
			var channel = new Channel("channel");
			var subject = new PubNubClient(channel);

			subject.Secured();

			Assert.True(channel.Secured);
		}
	}
}
