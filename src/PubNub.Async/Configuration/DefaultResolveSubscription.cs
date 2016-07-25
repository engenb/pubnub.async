using PubNub.Async.Models;
using PubNub.Async.Models.Subscribe;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.Subscribe;

namespace PubNub.Async.Configuration
{
	public class DefaultResolveSubscription : IResolveSubscription
	{
		private ICryptoService Crypto { get; }

		public DefaultResolveSubscription(ICryptoService crypto)
		{
			Crypto = crypto;
		}

		public Subscription<TMessage> Resolve<TMessage>(IPubNubEnvironment environment, Channel channel)
		{
			return new Subscription<TMessage>(environment, channel, Crypto);
		}
	}
}
