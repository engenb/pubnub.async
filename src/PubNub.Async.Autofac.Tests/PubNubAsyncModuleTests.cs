using Autofac;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;
using Xunit;

namespace PubNub.Async.Autofac.Tests
{
	public class PubNubAsyncModuleTests
	{
		[Fact]
		public void Module()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<PubNubAsyncModule>();
			builder.Build();

			Assert.IsType<PubNubAutofacEnvironment>(PubNub.Environment);
			
			var client = new PubNubClient("channel");

			var crypto = PubNub.Environment.Resolve<ICryptoService>(client);
			Assert.NotNull(crypto);
			
			var publish = PubNub.Environment.Resolve<IPublishService>(client);
			Assert.NotNull(publish);

			var access = PubNub.Environment.Resolve<IAccessManager>(client);
			Assert.NotNull(access);

			var history = PubNub.Environment.Resolve<IHistoryService>(client);
			Assert.NotNull(history);
		}
	}
}