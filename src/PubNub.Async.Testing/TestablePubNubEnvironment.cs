using System;
using Moq;
using PubNub.Async.Configuration;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Testing
{
	public class TestablePubNubEnvironment : DefaultPubNubEnvironment
	{
		public TestablePubNubEnvironment(
			Func<ICryptoService> cryptoFactory = null,
			Func<IPubNubClient, IAccessManager> accessFactory = null,
			Func<IPubNubClient, IHistoryService> historyFactory = null,
			Func<IPubNubClient, IPublishService> publishFactory = null )
		{
			cryptoFactory = cryptoFactory ?? Mock.Of<ICryptoService>;
			accessFactory = accessFactory ?? (client => Mock.Of<IAccessManager>());
			historyFactory = historyFactory ?? (client => Mock.Of<IHistoryService>());
			publishFactory = publishFactory ?? (client => Mock.Of<IPublishService>());

			Register(client => cryptoFactory());
			Register(client => accessFactory(client));
			Register(client => historyFactory(client));
			Register(client => publishFactory(client));
		}
	}
}