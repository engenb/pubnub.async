using System;
using Moq;
using PubNub.Async.Configuration;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Testing
{
	public class TestablePubNubSettings : AbstractPubNubSettings
	{
		public TestablePubNubSettings(
			Func<ICryptoService> cryptoFactory = null,
			Func<IPubNubClient, IAccessManager> accessFactory = null,
			Func<IPubNubClient, IHistoryService> historyFactory = null,
			Func<IPubNubClient, IPublishService> publishFactory = null )
		{
			CryptoFactory = cryptoFactory ?? Mock.Of<ICryptoService>;
			AccessFactory = accessFactory ?? (client => Mock.Of<IAccessManager>());
			HistoryFactory = historyFactory ?? (client => Mock.Of<IHistoryService>());
			PublishFactory = publishFactory ?? (client => Mock.Of<IPublishService>());
		}

		public override Func<ICryptoService> CryptoFactory { get; }
		public override Func<IPubNubClient, IAccessManager> AccessFactory { get; }
		public override Func<IPubNubClient, IHistoryService> HistoryFactory { get; }
		public override Func<IPubNubClient, IPublishService> PublishFactory { get; }
	}
}