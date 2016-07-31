using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using PubNub.Async.Configuration;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Autofac.Tests
{
	public class PubNubAutofacBootstrapperTests : AbstractTest, IDisposable
	{
		[Fact]
		public void Start__Given_Environment__When_EnvironmentCreated__Then_CopyValues()
		{

			var expectedAuthKey = Fixture.Create<string>();
			var expectedCipherKey = Fixture.Create<string>();
			var expectedMinutesToTimeout = Fixture.Create<int>();
			var expectedPublishKey = Fixture.Create<string>();
			var expectedSubscribeKey = Fixture.Create<string>();
			var expectedSecretKey = Fixture.Create<string>();
			var expectedSessionUuid = Fixture.Create<string>();
			var expectedSslEnabled = Fixture.Create<bool>();

			var expectedEnv = new PubNubAutofacEnvironment(Mock.Of<IComponentContext>())
			{
				AuthenticationKey = Fixture.Create<string>(),
				CipherKey = Fixture.Create<string>(),
				MinutesToTimeout = Fixture.Create<int>(),
				PublishKey = Fixture.Create<string>(),
				SubscribeKey = Fixture.Create<string>(),
				SecretKey = Fixture.Create<string>(),
				SessionUuid = Fixture.Create<string>(),
				SslEnabled = Fixture.Create<bool>()
			};
			var lazyEnv = new Lazy<IPubNubEnvironment>(() => expectedEnv);
			var autofacEnv = lazyEnv.Value;

			PubNub.Configure(c =>
			{
				c.AuthenticationKey = expectedAuthKey;
				c.CipherKey = expectedCipherKey;
				c.MinutesToTimeout = expectedMinutesToTimeout;
				c.PublishKey = expectedPublishKey;
				c.SubscribeKey = expectedSubscribeKey;
				c.SecretKey = expectedSecretKey;
				c.SessionUuid = expectedSessionUuid;
				c.SslEnabled = expectedSslEnabled;
			});
			var defaultEnv = PubNub.Environment;

			var subject = new PubNubAutofacBootstrapper(lazyEnv);

			Assert.IsType<DefaultPubNubEnvironment>(defaultEnv);

			subject.Start();

			var result = PubNub.Environment;

			Assert.IsType<PubNubAutofacEnvironment>(result);
			Assert.Equal(expectedAuthKey, result.AuthenticationKey);
			Assert.Equal(expectedCipherKey, result.CipherKey);
			Assert.Equal(expectedMinutesToTimeout, result.MinutesToTimeout);
			Assert.Equal(expectedPublishKey, result.PublishKey);
			Assert.Equal(expectedSubscribeKey, result.SubscribeKey);
			Assert.Equal(expectedSecretKey, result.SecretKey);
			Assert.Equal(expectedSessionUuid, result.SessionUuid);
			Assert.Equal(expectedSslEnabled, result.SslEnabled);
		}

		public void Dispose()
		{
			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => new DefaultPubNubEnvironment());
		}
	}
}
