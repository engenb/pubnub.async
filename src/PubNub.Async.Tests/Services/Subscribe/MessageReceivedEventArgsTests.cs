using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture;
using PubNub.Async.Services.Subscribe;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Services.Subscribe
{
	public class MessageReceivedEventArgsTests : AbstractTest
	{
		[Fact]
		public void ctor__Given_Arguments__Then_Initialize()
		{
			var expectedSubKey = Fixture.Create<string>();
			var expectedChannel = Fixture.Create<string>();
			var expectedSessionUuid = Fixture.Create<string>();
			var expectedSent = Fixture.Create<long>();
			var expectedMsgJson = JToken.Parse("{}");
			var expectedDecryptedMsgJson = JToken.Parse("{}");
			var expectedMessage = new object();

			var subject = new MessageReceivedEventArgs<object>(
				expectedSubKey,
				expectedChannel,
				expectedSessionUuid,
				expectedSent,
				expectedMsgJson,
				expectedDecryptedMsgJson,
				expectedMessage);

			Assert.Equal(expectedSubKey, subject.SubscribeKey);
			Assert.Equal(expectedChannel, subject.Channel);
			Assert.Equal(expectedSessionUuid, subject.SenderSessionUuid);
			Assert.Equal(expectedSent, subject.Sent);
			Assert.Equal(expectedMsgJson, subject.MessageJson);
			Assert.Equal(expectedDecryptedMsgJson, subject.DecryptedMessageJson);
			Assert.Equal(expectedMessage, subject.Message);
		}
	}
}
