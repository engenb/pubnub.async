using System;
using Autofac;
using PubNub.Async.Configuration;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;
using PubNub.Async.Services.Subscribe;
using Xunit;

namespace PubNub.Async.Autofac.Tests
{
	public class PubNubAsyncModuleTests : IDisposable
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

		    var subscribe = PubNub.Environment.Resolve<ISubscribeService>(client);
            Assert.NotNull(subscribe);
            
			var history = PubNub.Environment.Resolve<IHistoryService>(client);
			Assert.NotNull(history);

			var resolveSub = PubNub.Environment.Resolve<IResolveSubscription>(client);
			Assert.NotNull(resolveSub);

			var sub = resolveSub.Resolve<object>(client.Environment, client.Channel);
			Assert.NotNull(sub);
		}

		public void Dispose()
		{
			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => new DefaultPubNubEnvironment());
		}
	}
}