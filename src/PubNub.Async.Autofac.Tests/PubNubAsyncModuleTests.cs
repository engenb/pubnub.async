using Autofac;
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

			Assert.IsType<PubNubAutofacSettings>(PubNub.GlobalSettings);

			var crypto = PubNub.GlobalSettings.CryptoFactory();
			Assert.NotNull(crypto);

			var client = new PubNubClient("channel");
			var history = PubNub.GlobalSettings.HistoryFactory(client);
			Assert.NotNull(history);
		}
	}
}